using NLog.Internal;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Web.Helpers;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Collections;
using System.Reflection;
using nsteam.Commons;

namespace nsteam.ConfigServer.Types
{
    public class ConfigRepository
    {
        private Dictionary<string, ConfigEntity> _sources;

        private int NumberofChanges = 0;
        private ILoggerService _logger;


        private int MaxRecursiveProcessing = 5;
        private bool fullStoryObjectInfo = false;
        private static object objlock = new object();
        private ConfigWatcher watcher = new ConfigWatcher();

        #region Support Methods



        private void LoadDocument(string groupName = null)
        {
            watcher.StopMonitoring();
            _logger.Log("Loading configuration...", EventType.Info);
            var cfg = new ConfigSettings();

            foreach (var source in cfg.SourceElements)
            {
                if (groupName != null)
                {
                    if (source.name.ToUpper() != groupName.ToUpper()) continue;
                }


                string filename = source.file;
                if ((!filename.Contains(":")))
                    filename = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), filename);

                

                try
                {
                    if (!File.Exists(filename))
                    {
                        _logger.Log("Can't find source: " + filename, EventType.Fatal);
                        return;
                    }
                    else
                    {
                        _logger.Log("Source: " + filename, EventType.Info);
                    }
                    watcher.RemoveGroupWatcher(groupName);

                    if (!_sources.ContainsKey(source.name))
                    {
                        _sources.Add(source.name, new ConfigEntity());
                        _sources[source.name].FileName = filename;
                        _sources[source.name].GroupName = source.name;
                    }

                    if (Path.GetExtension(filename) == ".json")
                    {
                        lock (objlock)
                        {
                            var jsonContent = string.Empty;
                            TryerHelper.Try(delegate { jsonContent = File.ReadAllText(filename); }).Retry(5).RetryInterval(1000).IgnoreException(typeof(IOException)).Execute();
                            jsonContent = Regex.Replace(jsonContent, @"("")(\w+)(""\s*)(:)(\s*"")", "$1@$2$3$4$5"); // Generate attributes based on single nodes                        
                            _sources[source.name].Root = JsonConvert.DeserializeXmlNode(jsonContent.Replace(@"\", @"\\"), null, true);
                        }
                    }
                    else
                    {
                        lock (objlock)
                        {
                            _sources[source.name].Root = new XmlDocument();
                            _sources[source.name].Root.Load(filename);
                        }
                    }

                    watcher.AddToGroupWatcher(groupName, filename);

                    // Preprocess Tree References and Inheritances
                    lock (objlock)
                    {
                        string json = Newtonsoft.Json.JsonConvert.SerializeXmlNode(_sources[source.name].Root.ChildNodes[0], Newtonsoft.Json.Formatting.None);
                        json = Regex.Replace(json, "(?<=\")(@)(?!.*\":\\s )", string.Empty, RegexOptions.IgnoreCase); //Remove "@" from attributes
                        dynamic obj = Json.Decode(json);

                        var _roofr = _sources[source.name].Root;

                        ProcessReferences(source.name, ref _roofr, obj, obj, MaxRecursiveProcessing);
                        ProcessInheritances(source.name, ref _roofr, obj, obj, MaxRecursiveProcessing);

                        string new_json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);

                        new_json = Regex.Replace(new_json, @"("")(\w+)(""\s*)(:)(\s*"")", "$1@$2$3$4$5"); // Generate attributes based on single nodes
                        _sources[source.name].RootReferences = Newtonsoft.Json.JsonConvert.DeserializeXmlNode(new_json, null, true);
                    }
                    _logger.Log("Configuration loaded.", EventType.Info);
                }
                
                catch (Exception ex)
                {
                    _logger.Log("Configuration failed>" + ex.ToString(), EventType.Error);
                }                
            }

            watcher.StartMonitoring();
        }
        
        private XmlDocument _repo(string name)
        {
            if (!_sources.ContainsKey(name))
                LoadDocument(name);
            return _sources[name].Root;
        }

        private XmlDocument _repo_with_references(string name)
        {

            if (!_sources.ContainsKey(name))
                LoadDocument(name);
            return _sources[name].RootReferences;
        }

        // Define the event handlers.
        private void OnConfigurationChanged(object source, FileSystemEventArgs e)
        {            
            lock (objlock)
            {
                //if ((e.ChangeType == WatcherChangeTypes.Changed) && (NumberofChanges < 1)) // This version of filewatcher only fires once
                if ((e.ChangeType == WatcherChangeTypes.Changed))  
                {
                    _logger.Log("Configuration file changed.", EventType.Warn);
                    //_root = null; // invalidate the document and reload                    
                    LoadDocument();                    
                }

                NumberofChanges++;
                if (NumberofChanges == 3)
                    NumberofChanges = 0;
            }
        }

        private string NormalizeXPath(string path)
        {
            string xpath = path; // enviroment[@Production].application[@app1]            
            xpath = Regex.Replace(xpath, @"(\[@)(\w+)(\])", "$1name='$2'$3"); // Simplify @name=""
            xpath = xpath.Replace(".", "/");
            return xpath;
        }

        private dynamic Clone(dynamic obj)
        {
            string json = Json.Encode(obj);
            return Json.Decode(json);
        }

        private void AddInheritedInfo(dynamic obj, string fieldname, dynamic value, string inherits, bool overrided)
        {
            ObjectInfo info = (ObjectInfo)obj["objectinfo"];
            if (info == null) { info = new ObjectInfo(); obj["objectinfo"] = info; } 
            ObjectInfo.InheritedField inhf = new ObjectInfo.InheritedField();
            inhf.PropertyName = fieldname;
            inhf.PropertyInheritedValue = Newtonsoft.Json.JsonConvert.SerializeObject(value);
            inhf.InheritedFrom = inherits;
            inhf.Overrided = overrided;


            if (fullStoryObjectInfo) info.InheritedFields.Add(inhf);
            else
            {
                var pp = info.InheritedFields.FirstOrDefault(p => p.PropertyName == inhf.PropertyName);
                if (pp != null)
                {
                    if (inhf.Overrided) { 
                        info.InheritedFields.Remove(pp); // Remove the previous
                        info.InheritedFields.Add(inhf);  // Insert the new override
                    }                    
                } else
                    info.InheritedFields.Add(inhf);

            }
        }

        private void AddReferencedInfo(dynamic obj, string fieldname, string from)
        {
            ObjectInfo info = (ObjectInfo)obj["objectinfo"];
            if (info == null) { info = new ObjectInfo(); obj["objectinfo"] = info; }
            ObjectInfo.ReferencedField inhf = new ObjectInfo.ReferencedField();
            inhf.PropertyName = fieldname;
            inhf.Reference = from;
            info.ReferencedFields.Add(inhf);
        }

        private void RemoveMember(dynamic obj, string membername)
        {
            FieldInfo[] fields = typeof(DynamicJsonObject).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo fi = fields.FirstOrDefault(n => n.Name == "_values");
            IDictionary<string, object> d = fi.GetValue(obj);
            d.Remove(membername);
        }

        private void UpdateJsonArrayValues(DynamicJsonArray arr, List<object> list)
        {
            FieldInfo[] fields = typeof(DynamicJsonArray).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo fi = fields.FirstOrDefault(n => n.Name == "_arrayValues");
            object[] d = (object[])fi.GetValue(arr);
            fi.SetValue(arr, list.ToArray());
        }

        private void MergeObjects(dynamic obj, dynamic baseobj, string inherits)
        {
            // class obj : baseobj
            if (baseobj is ObjectInfo)
            {
                if (!(obj is ObjectInfo)) throw new ApplicationException("Type mistmatch; obj expected as ObjectInfo.");
                var baseobjinfo = baseobj as ObjectInfo;
                var objinfo = obj as ObjectInfo;

                foreach(var reff in baseobjinfo.ReferencedFields){
                    objinfo.ReferencedFields.Insert(0, reff);
                }
                foreach (var inh in baseobjinfo.InheritedFields)
                {
                    if (fullStoryObjectInfo) objinfo.InheritedFields.Insert(0, inh);
                    else
                    {
                        var pp = objinfo.InheritedFields.FirstOrDefault(p => p.PropertyName == inh.PropertyName);
                        if (pp != null)
                        {
                            if (!pp.Overrided)
                            {
                                objinfo.InheritedFields.Remove(pp); // Remove the previous
                                objinfo.InheritedFields.Insert(0, inh);  // Insert the new override
                            }
                        }
                        else
                            objinfo.InheritedFields.Insert(0, inh);
                    }

                }

            } else
            if (baseobj is DynamicJsonArray)
            {
                if (!(obj is DynamicJsonArray)) throw new ApplicationException("Type mistmatch; obj expected as array.");                
                var obj_element_list = (obj as DynamicJsonArray).ToList();
                foreach (var elem in baseobj)
                {
                    // identify the array element by "name" property, if the element does not have "name" it cant be identified
                    var obj_found = obj_element_list.FirstOrDefault(e => ((dynamic)e)["name"] == elem["name"]);
                    if (obj_found != null)
                        MergeObjects(obj_found, elem, inherits + "[@" + elem["name"] + "]");
                    else
                    {
                        obj_element_list.Add(Clone(elem));
                        UpdateJsonArrayValues(obj, obj_element_list);
                    }
                }
            }
            else if (baseobj is DynamicJsonObject)
            {
                IEnumerable<string> baseobjProp = baseobj.GetDynamicMemberNames();
                IEnumerable<string> objProp = obj.GetDynamicMemberNames();
                foreach (string nprop in baseobjProp)
                {
                    if (!objProp.Contains(nprop))
                    {
                        // member do not exist in current obj
                        obj[nprop] = Clone(baseobj[nprop]);
                        AddInheritedInfo(obj, nprop, baseobj[nprop], inherits, false);
                    }
                    else
                    {
                        MergeObjects(obj[nprop], baseobj[nprop], inherits + "." + nprop);
                        if ((nprop != "objectinfo") && (!(obj[nprop] is DynamicJsonObject)))
                        {
                            dynamic value = baseobj[nprop];
                            if (obj[nprop] is DynamicJsonArray) value = "<array>";
                            AddInheritedInfo(obj, nprop, value, inherits, true);
                        }
                    }
                }
            }
        }

        private void ProcessInheritances(string name, ref XmlDocument target, dynamic rootobj, dynamic obj, int recursivelyMax)
        {
            if (recursivelyMax == 0) return;
            if (obj is DynamicJsonObject)
            {
                bool modified = false;
                do
                {
                    modified = false;
                    IEnumerable<string> propertyNames = obj.GetDynamicMemberNames();
                    foreach (string prop in propertyNames)
                    {
                        if (obj[prop] is DynamicJsonObject) ProcessInheritances(name, ref target, rootobj, obj[prop], recursivelyMax - 1);
                        else if (obj[prop] is DynamicJsonArray)
                        {
                            foreach (var elem in obj[prop])
                            {
                                ProcessInheritances(name, ref target, rootobj, elem, recursivelyMax - 1);
                            }
                        }
                        else if (obj[prop] is string)
                        {
                            string refId = obj[prop];
                            if (refId.StartsWith("*") && (prop == "inherits"))
                            {
                                try
                                {
                                    dynamic baseobj = InternalGetNodes(name, target, refId.Substring(1), recursivelyMax - 1);
                                    MergeObjects(obj, baseobj, obj["inherits"]);
                                    RemoveMember(obj, "inherits");
                                    modified = true;
                                    break;
                                }
                                catch
                                {
                                    obj[prop] = obj[prop] + "--[Error]";
                                }
                            }
                        }
                    }
                } while (modified);
            }

        }

        private void ProcessReferences(string name, ref XmlDocument target, dynamic rootobj, dynamic obj, int recursivelyMax)
        {
            if (recursivelyMax == 0) return;
            if (obj is DynamicJsonObject)
            {
                bool modified = false;
                do
                {
                    modified = false;
                    IEnumerable<string> propertyNames = obj.GetDynamicMemberNames();
                    foreach (string prop in propertyNames)
                    {
                        if (obj[prop] is DynamicJsonArray)
                        {
                            foreach (var elem in obj[prop])
                            {
                                ProcessReferences(name, ref target, rootobj, elem, recursivelyMax - 1);
                            }
                        } 
                        else if (obj[prop] is DynamicJsonObject) ProcessReferences(name, ref target, rootobj, obj[prop], recursivelyMax - 1);
                        
                        else if (obj[prop] is string)
                        {
                            string refId = obj[prop];
                            if (refId.StartsWith("*") && (prop.StartsWith("include")))
                            {
                                try {
                                    // Attempt to load a file
                                    var filepath = refId.Substring(1);
                                    if ((!filepath.Contains(":")))
                                        filepath = Path.Combine(Path.GetDirectoryName(_sources[name].FileName), filepath);

                                    string jsonContent = File.ReadAllText(filepath);

                                    watcher.AddToGroupWatcher(name, filepath);

                                    dynamic newobj = Json.Decode(jsonContent);
                                    MergeObjects(obj, newobj, refId);
                                    RemoveMember(obj, prop);

                                    string json_target = Json.Encode(rootobj);

                                    json_target = json_target.RemoveObjectInfo();

                                    target = json_target.CreateXMLDocument();

                                    modified = true;
                                    break;
                                }
                                catch(Exception ex)
                                {
                                    obj[prop] = obj[prop] + "--[Error]";
                                    _logger.Log(ex.Message, EventType.Error);
                                }
                            }
                            else
                            if (refId.StartsWith("*") && (prop != "inherits") && (!prop.StartsWith("include")))
                            {
                                try
                                {
                                    dynamic newobj = InternalGetNodes(name, target, refId.Substring(1), recursivelyMax - 1);
                                    if (newobj == null)
                                    {
                                        // Attempt to load a file
                                        var filepath = refId.Substring(1);
                                        if ((!filepath.Contains(":")))
                                            filepath = Path.Combine(Path.GetDirectoryName(_sources[name].FileName), filepath);

                                        string jsonContent = File.ReadAllText(filepath);

                                        watcher.AddToGroupWatcher(name, filepath);

                                        newobj = Json.Decode(jsonContent);
                                    }
                                    obj[prop] = newobj;

                                    target = Extensions.CreateXMLDocument(rootobj);

                                    AddReferencedInfo(obj, prop, refId);
                                    modified = true;
                                    break;
                                }
                                catch (Exception ex)
                                {
                                    obj[prop] = obj[prop] + "--[Error]";
                                    _logger.Log(ex.Message, EventType.Error);
                                }
                            }
                        }
                    }
                } while (modified);
            }
        }

        private dynamic InternalGetNodes(string name, XmlDocument target, string path, int recursivelyReferencesMax)
        {
            string xpath = NormalizeXPath(path);

            string lastpart = path.Substring(path.LastIndexOf(".") + 1);
            bool findSingle = false;
            if (lastpart.IndexOf("[") >= 0)
            {
                lastpart = lastpart.Remove(lastpart.IndexOf("["));
                findSingle = true;
            }

            XmlNodeList titleNodeList = (xpath == "/") ? target.ChildNodes : target.SelectNodes(xpath);

            if ((titleNodeList == null) || (titleNodeList.Count == 0))
            {
                if (!lastpart.StartsWith("@"))
                {
                    lastpart = "@" + lastpart;
                    return InternalGetNodes(name, target, path.Remove(path.LastIndexOf(".") + 1) + lastpart, recursivelyReferencesMax);
                }
                else return null;
            }

            List<dynamic> list = new List<dynamic>();
            bool isArrayType = false;
            foreach (XmlNode node in titleNodeList)
            {
                if (node.NodeType == XmlNodeType.XmlDeclaration) continue;
                if ((!findSingle) && (node.Attributes != null) && (node.Attributes["json:Array"] != null) && (node.Attributes["json:Array"].Value == "true")) isArrayType = true;
                string json = Newtonsoft.Json.JsonConvert.SerializeXmlNode(node, Newtonsoft.Json.Formatting.None);
                json = Regex.Replace(json, "(?<=\")(@)(?!.*\":\\s )", string.Empty, RegexOptions.IgnoreCase); //Remove "@" from attributes
                lastpart = lastpart.Replace("@", "");
                if (lastpart != "/")
                    list.Add(Json.Decode(json)[lastpart]);
                else
                    list.Add(Json.Decode(json));
            }

            if (recursivelyReferencesMax > 0)
            {
                foreach (var elem in list)
                    ProcessReferences(name, ref target, elem, elem, recursivelyReferencesMax);

                foreach (var elem in list)
                    ProcessInheritances(name, ref target, elem, elem, recursivelyReferencesMax);
            }

            if ((list.Count > 1) || (isArrayType))
                return new DynamicJsonArray(list.ToArray());
            else
                return list[0];
        }

        #endregion

        public ConfigRepository(ILoggerService logger)
        {
            _logger = logger;
            _sources = new Dictionary<string, ConfigEntity>();
            watcher.Change += Watcher_Change;
        }

        private void Watcher_Change(string groupName, string fileName)
        {
            _logger.Log("Change: " + groupName + " file: " + Path.GetFileName(fileName), EventType.Warn);
            _sources[groupName].Root = null;
            _sources[groupName].RootReferences = null;
            LoadDocument(groupName);            
        }

        public dynamic GetTree(string name, string path)
        {
            XmlDocument target = _repo_with_references(name);
            var node = InternalGetNodes(name, target, path, 0);
            return node;
        }

        public TNode GetNode(string name, string path)
        {
            XmlDocument target = _repo(name);
            var node = InternalGetNodes(name, target, path, 0);
            return new TNode(path, node);
        }

        public void SetNode(string name, TNode node)
        {
            string path = node.Path;
            dynamic obj = node.Value;
            string xpath = NormalizeXPath(path);
            lock (objlock)
            {
                XmlNodeList titleNodeList = (xpath == "/") ? _repo(name).ChildNodes : _repo(name).SelectNodes(xpath);
                if ((titleNodeList == null) || (titleNodeList.Count == 0)) throw new ApplicationException("Path not found");
                List<XmlNode> list = new List<XmlNode>();
                foreach (XmlNode n in titleNodeList)
                {
                    if (n.NodeType == XmlNodeType.XmlDeclaration) continue;
                    list.Add(n);
                }
                string lastpart = path.Substring(path.LastIndexOf(".") + 1);
                if (lastpart.IndexOf("[") >= 0) lastpart = lastpart.Remove(lastpart.IndexOf("["));
                if (xpath == "/") lastpart = null;
                string str = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                str = Regex.Replace(str, @"("")(\w+)(""\s*)(:)(\s*"")", "$1@$2$3$4$5"); // Generate attributes based on single nodes
                var parentNode = list[0].ParentNode;

                if ((list.Count > 1) || (list[0].Attributes["json:Array"] != null))
                {
                    var doc = JsonConvert.DeserializeXmlNode("{\"" + lastpart + "\" : " + str + "}", lastpart, true);
                    foreach (XmlNode n in doc.ChildNodes[0].ChildNodes)
                    {
                        XmlNode importNode = parentNode.OwnerDocument.ImportNode(n, true);
                        if (importNode.Attributes["json:Array"] == null)
                        {
                            XmlAttribute attr = parentNode.OwnerDocument.CreateAttribute("json:Array", "http://james.newtonking.com/projects/json");
                            attr.Value = "true";
                            importNode.Attributes.Append(attr);
                        }
                        parentNode.InsertBefore(importNode, list[0]);
                    }

                    foreach (XmlNode xmlnode in list)
                        parentNode.RemoveChild(xmlnode);
                }
                else
                {
                    XmlNode xmlnode = list[0];
                    var doc = JsonConvert.DeserializeXmlNode(str, lastpart, true);
                    XmlNode importNode = xmlnode.OwnerDocument.ImportNode(doc.DocumentElement, true);
                    if ((xmlnode.Attributes != null) && (xmlnode.Attributes["json:Array"] != null) && (xmlnode.Attributes["json:Array"].Value == "true"))
                        importNode.Attributes.Append(xmlnode.Attributes["json:Array"]);

                    parentNode.ReplaceChild(importNode, xmlnode);
                }
                try
                {
                    watcher.StopMonitoring(name);
                    CreateBackup(name);
                    SaveConfig(name);
                    _sources[name].Root = null;
                    LoadDocument();
                    watcher.StartMonitoring(name);
                }
                catch (Exception ex)
                {
                    watcher.StopMonitoring(name);
                    _logger.Log(ex.Message, EventType.Error);
                    throw ex;
                }
            }
        }

        public string CreateBackup(string name)
        {

            string newfilename = Path.Combine(Path.GetDirectoryName(_sources[name].FileName), Path.GetFileNameWithoutExtension(_sources[name].FileName) + DateTime.Now.ToString("yyyy-MM-dd hh-mm-ss.ffff") + Path.GetExtension(_sources[name].FileName));
            _logger.Log("Create backup: " + Path.GetFileName(newfilename), EventType.Warn);
            File.Copy(_sources[name].FileName, newfilename);
            return newfilename;
        }

        public void SaveConfig(string name)
        {
            if (Path.GetExtension(_sources[name].FileName) == ".json")
            {
                string json = Newtonsoft.Json.JsonConvert.SerializeXmlNode(_repo(name), Newtonsoft.Json.Formatting.Indented);
                json = Regex.Replace(json, "(?<=\")(@)(?!.*\":\\s )", string.Empty, RegexOptions.IgnoreCase); //Remove "@" from attributes
                _logger.Log("Saving config... ", EventType.Warn);
                File.WriteAllText(_sources[name].FileName, json);
            }
            else
                _repo(name).Save(_sources[name].FileName);
        }
        
    }
}
