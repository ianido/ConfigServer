using ICSharpCode.Addons;
using ICSharpCode.AddOns;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using NLog;
using nsteam.ConfigServer.Client;
using nsteam.Windows.Commons;
using nsteam.Windows.Commons.Controls.Grid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Helpers;
using System.Windows.Forms;
using System.Xml;

namespace nsteam.ConfigServer
{
    public partial class MainForm : Form
    {
        private Logger _logger = LogManager.GetCurrentClassLogger();

        private static string _jsonarrayobj = "<collection>";
        private static string _jsonarrayitemobj = "<collectionelement>";
        private static string _jsonobj = "<object>";
        private TNode _obj = null;
        private string _serverurl = "";

        private ConfigService srv = null;

        List<PropertyValue> list = new List<PropertyValue>();
        
        public MainForm()
        {
            InitializeComponent();
            
        }

        #region Event Handlers

        private void LoadServers()
        {
            NameValueCollection servers = (NameValueCollection)ConfigurationManager.GetSection("servers");

            foreach (var server in servers.AllKeys)
            {
                ToolStripMenuItem submenu = new System.Windows.Forms.ToolStripMenuItem();
                submenu.Text = server + "(" + servers[server] + ")";
                submenu.Tag = (string) servers[server];
                submenu.Click += submenu_Click;

                openUrlToolStripMenuItem.DropDownItems.Add(submenu);
            }
        }

        void submenu_Click(object sender, EventArgs e)
        {           
            try
            {
                _serverurl = (string)((ToolStripMenuItem)sender).Tag;
                srv = new ConfigService(_serverurl);
                _obj = srv.GetNode();
                _obj.Value = Json.Decode(Newtonsoft.Json.JsonConvert.SerializeObject(_obj.Value));
                //_obj.Value = new DynamicJsonObject(_obj.Value);
                tree.Populate(_obj.Value);
                //FormatSource(_obj, txtSource, rdAsXML.Checked);
                Pristine();
            }
            catch (Exception ex)
            {
                Error(ex.Message, ex.ToString());
            }            
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                panelTopGrid.Visible = splitterGrid.Visible = false;
                tabControl1.TabPages.Remove(tabSource);
                LoadServers();                              

            }
            catch (Exception ex)
            {
                _logger.FatalException("GetSetting Error.", ex);
            }
        }

        private void tree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null) return;
            list = new List<PropertyValue>();
            PropertyValue item = ((PropertyValue)e.Node.Tag);

            dynamic obj = item.obj;
            if (obj is DynamicJsonArray)
            {
                foreach (var elem in (obj as DynamicJsonArray))
                {
                    PropertyValue pv = new PropertyValue() { obj = elem, Property = item.Property, col = obj, OldProperty = item.Property };
                    if (elem is string) pv.Value = (string)elem;
                    else if (elem is IEnumerable) pv.Value = _jsonarrayobj;
                    else
                    {
                        string pname = ((dynamic)elem)["name"];
                        if (pname != null)
                            pv.Value = _jsonarrayitemobj + "@" + pname;
                        else
                            pv.Value = _jsonarrayitemobj;
                    }
                    list.Add(pv);
                }
            }

            IEnumerable<string> propertyNames = obj.GetDynamicMemberNames();
            foreach (string prop in propertyNames)
            {
                PropertyValue pv = new PropertyValue() { obj = obj, Property = prop, OldProperty = prop };

                if (obj[prop] is string) pv.Value = (string)obj[prop];
                else if (obj[prop] is IEnumerable) pv.Value = _jsonarrayobj;
                else
                {
                    string pname = ((dynamic)obj[prop])["name"];
                    if (pname != null)
                        pv.Value = _jsonobj + "@" + pname;
                    else
                        pv.Value = _jsonobj ;
                }
                list.Add(pv);
            }

            grid.DataSource = list;
            SetStatusBar();
            SetPreview();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (grid.SelectedRows.Count > 0)
            {
                PropertyValue item = ((PropertyValue)grid.SelectedRows[0].DataBoundItem);
                if (item == null) return;
                Remove(item, null);

                int itemToSelect = grid.SelectedRows[0].Index - 1;
                if (itemToSelect < 0) itemToSelect = 0;

                grid.Rows[itemToSelect].Selected = true;

                tree.RefreshNode(tree.SelectedNode);
                tree_AfterSelect(tree, new TreeViewEventArgs(tree.SelectedNode));
                //FormatSource(_obj, txtSource, rdAsXML.Checked);
                SetStatusBar();
                Dirty();
            }
        }

        private void btnRemoveNode_Click(object sender, EventArgs e)
        {
            PropertyValue item = (PropertyValue)tree.SelectedNode.Tag;
            if (item == null) return;

            TreeNode pnode = tree.SelectedNode.Parent;

            Remove(item, (PropertyValue)pnode.Tag);

            tree.RefreshNode(pnode);
            tree_AfterSelect(tree, new TreeViewEventArgs(pnode));
            tree.SelectedNode = pnode;

            //FormatSource(_obj, txtSource, rdAsXML.Checked);
            SetStatusBar();
            Dirty();
        }

        private void btnSaveTree_Click(object sender, EventArgs e)
        {
            try
            {
                srv.SaveNode(_obj);
                SetPreview();
                Pristine();
            }
            catch (Exception ex)
            {
                Error(ex.Message, ex.ToString());
            }
            
        }

        private void grid_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            PropertyValue item = (PropertyValue)grid[e.ColumnIndex, e.RowIndex].OwningRow.DataBoundItem;

            FieldInfo[] fields = typeof(DynamicJsonObject).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo fi = fields.FirstOrDefault(n => n.Name == "_values");


            if ((item.Value.Contains(_jsonarrayobj)) || (item.Value.Contains(_jsonobj)) || (item.Value.Contains(_jsonarrayitemobj)))
            {
                if (item.OldProperty != item.Property)
                {
                    if (item.Value.Contains(_jsonarrayitemobj))
                    {
                        var pnode = tree.SelectedNode.Parent;
                        PropertyValue pitem = (PropertyValue)pnode.Tag;

                        IDictionary<string, object> d = fi.GetValue(pitem.obj);
                        dynamic savedobj = pitem.obj[item.OldProperty];
                        if (savedobj != null)
                        {
                            Remove(item, pitem);
                            d[item.Property] = savedobj;
                        }
                        tree.SelectedNode = pnode; 
                        tree.RefreshNode(pnode);

                        this.BeginInvoke(new MethodInvoker(() =>
                        {
                            tree_AfterSelect(tree, new TreeViewEventArgs(pnode));
                        }));
                                                                
                    }
                    else
                    {
                        IDictionary<string, object> d = fi.GetValue(item.obj);
                        dynamic savedobj = item.obj[item.OldProperty];
                        if (savedobj != null)
                        {
                            Remove(item, null);
                            d[item.Property] = savedobj;
                        }
                        tree.RefreshNode(tree.SelectedNode);
                    }                    
                }               
            } 
            else
            {
                IDictionary<string, object> d = fi.GetValue(item.obj);
                if (item.OldProperty != item.Property)
                {
                    Remove(item, null);
                }
                d[item.Property] = item.Value;
                tree.RefreshNode(tree.SelectedNode);                
            }

            //FormatSource(_obj, txtSource, rdAsXML.Checked);
        }

        private void grid_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (Char)Keys.Delete) btnDelete_Click(tree, new EventArgs());
        }

        private void grid_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach (var row in grid.Rows)
            {
                PropertyValue item = (PropertyValue)((DataGridViewRow)row).DataBoundItem;
                if ((item.Value.Contains(_jsonarrayobj)) || (item.Value.Contains(_jsonobj)) || (item.Value.Contains(_jsonarrayitemobj)))
                {
                    ((DataGridViewRow)row).Cells[2].ReadOnly = true;
                    if (item.Value.Contains(_jsonarrayitemobj)) ((DataGridViewRow)row).Cells[0].ReadOnly = true;
                    
                }
                if  (item.Property.ToLower() == "name")
                {
                    ((DataGridViewRow)row).Cells[0].ReadOnly = true;

                }
            }

        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            PropertyValue item = (PropertyValue)tree.SelectedNode.Tag;
            if (item.obj is DynamicJsonArray)
            {
                AddCollectionNode(item);
                tree.RefreshNode(tree.SelectedNode);
            }
            else
            {

                IEnumerable<string> propertyNames = item.obj.GetDynamicMemberNames();

                string extra = ""; int i = 1;
                while (propertyNames.FirstOrDefault(p => p == "NewKey" + extra) != null)
                {
                    extra = i.ToString();
                    i++;
                }

                item.obj["NewKey" + extra] = "NewValue";
            }

            tree_AfterSelect(tree, new TreeViewEventArgs(tree.SelectedNode));
            //FormatSource(_obj, txtSource, rdAsXML.Checked);
            SetStatusBar();
            Dirty();
        }

        private void btnAddNode_Click(object sender, EventArgs e)
        {
            PropertyValue item = (PropertyValue)tree.SelectedNode.Tag;
            if (item.obj is DynamicJsonArray)
                AddCollectionNode(item);
            else
                AddBasicNode(item);
            
            tree.RefreshNode(tree.SelectedNode);
            tree_AfterSelect(tree, new TreeViewEventArgs(tree.SelectedNode));
            //FormatSource(_obj, txtSource, rdAsXML.Checked);
            SetStatusBar();
            Dirty();
        }

        /// <summary>
        /// Add item to a collection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddCollection_Click(object sender, EventArgs e)
        {
            PropertyValue item = (PropertyValue)tree.SelectedNode.Tag;

            if (item.obj is DynamicJsonArray) return;

            IEnumerable<string> propertyNames = item.obj.GetDynamicMemberNames();
            string extra = ""; int i = 1;
            while (propertyNames.FirstOrDefault(p => p == "NewArray" + extra) != null)
            {
                extra = i.ToString();
                i++;
            }
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict.Add("name", "new");
            DynamicJsonObject jsonobj = new DynamicJsonObject(dict);
            DynamicJsonArray jsonarr = new DynamicJsonArray(new[]{jsonobj});
            item.obj["NewArray" + extra] = jsonarr;



            tree.RefreshNode(tree.SelectedNode);
            tree_AfterSelect(tree, new TreeViewEventArgs(tree.SelectedNode));
            //FormatSource(_obj, txtSource, rdAsXML.Checked);
            SetStatusBar();
            Dirty();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox box = new AboutBox();
            box.ShowDialog();
        }

        private void rdAsJson_CheckedChanged(object sender, EventArgs e)
        {
            bool pstatus = statusPristine.Visible;
            FormatSource(_obj, txtSource, rdAsXML.Checked);
            if (pstatus) Pristine();
        }

        private void btnSaveSource_Click(object sender, EventArgs e)
        {
            try
            {
                XmlDocument doc = null;
                if (rdAsXML.Checked)
                {
                    doc = new XmlDocument();
                    doc.LoadXml(txtSource.Text);

                }
                else
                {
                    var jsonContent = txtSource.Text;
                    jsonContent = Regex.Replace(jsonContent, @"("")(\w+)(""\s*)(:)(\s*"")", "$1@$2$3$4$5"); // Generate attributes based on single nodes                        
                    doc = JsonConvert.DeserializeXmlNode(txtSource.Text, null, true);
                }

                string json = Newtonsoft.Json.JsonConvert.SerializeXmlNode(doc.ChildNodes[0], Newtonsoft.Json.Formatting.None);
                json = Regex.Replace(json, "(?<=\")(@)(?!.*\":\\s )", string.Empty, RegexOptions.IgnoreCase); //Remove "@" from attributes
                _obj.Value = Json.Decode(json);


                srv.SaveNode(_obj);
                tree.Populate(_obj.Value);
                tree_AfterSelect(tree, new TreeViewEventArgs(tree.SelectedNode));
                FormatSource(_obj, txtSource, rdAsXML.Checked);
                Pristine();
            }
            catch (Exception ex)
            {
                Error(ex.Message, ex.ToString());
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Dirty();
        }

        private void statusLabel_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab(tabQuery);
            txtCommand.Text = statusLabel.Text;
            btnGetTree_Click(btnGetNode, null);
        }

        #endregion

        #region Methods

        public static String PrintXML(XmlDocument document)
        {
            String Result = "";

            MemoryStream mStream = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(mStream, Encoding.Unicode);

            try
            {

                writer.Formatting = System.Xml.Formatting.Indented;

                // Write the XML into a formatting XmlTextWriter
                document.WriteContentTo(writer);
                writer.Flush();
                mStream.Flush();

                // Have to rewind the MemoryStream in order to read
                // its contents.
                mStream.Position = 0;

                // Read MemoryStream contents into a StreamReader.
                StreamReader sReader = new StreamReader(mStream);

                // Extract the text from the StreamReader.
                String FormattedXML = sReader.ReadToEnd();

                Result = FormattedXML;
            }
            catch (XmlException)
            {
            }

            mStream.Close();
            writer.Close();
            return Result;
        }

        private void Remove(PropertyValue item, PropertyValue parentitem)
        {
            if (item == null) return;

            if (item.col != null)
            {
                var collection = (item.col as DynamicJsonArray);
                FieldInfo[] fields = typeof(DynamicJsonArray).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo fi = fields.FirstOrDefault(n => n.Name == "_arrayValues");
                object[] d = fi.GetValue(item.col);

                var foos = new List<object>(d);
                foos.Remove(item.obj);
                fi.SetValue(item.col, foos.ToArray());
            }
            else if (parentitem != null)
            {
                FieldInfo[] fields = typeof(DynamicJsonObject).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo fi = fields.FirstOrDefault(n => n.Name == "_values");
                IDictionary<string, object> d = fi.GetValue(parentitem.obj);
                d.Remove(item.OldProperty);
            }
            else
            {
                FieldInfo[] fields = typeof(DynamicJsonObject).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo fi = fields.FirstOrDefault(n => n.Name == "_values");
                IDictionary<string, object> d = fi.GetValue(item.obj);
                d.Remove(item.OldProperty);
            }
        }

        private void AddBasicNode(PropertyValue item)
        {
            IEnumerable<string> propertyNames = item.obj.GetDynamicMemberNames();
            string extra = ""; int i = 1;
            while (propertyNames.FirstOrDefault(p => p == "NewKey" + extra) != null)
            {
                extra = i.ToString();
                i++;
            }
            DynamicJsonObject jsonobj = new DynamicJsonObject(new Dictionary<string, object>());
            item.obj["NewKey" + extra] = jsonobj;
        }

        private void AddCollectionNode(PropertyValue item)
        {
            if (item.obj is DynamicJsonArray)
            {
                var collection = (item.obj as DynamicJsonArray);
                FieldInfo[] fields = typeof(DynamicJsonArray).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo fi = fields.FirstOrDefault(n => n.Name == "_arrayValues");
                object[] d = fi.GetValue(item.obj);

                var foos = new List<object>(d);


                if (foos.Count > 0)
                {
                    // Take the last element of the array
                    var last = foos[foos.Count - 1];
                    FieldInfo[] lfields = typeof(DynamicJsonObject).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
                    FieldInfo lfi = lfields.FirstOrDefault(n => n.Name == "_values");
                    IDictionary<string, object> ld = (IDictionary<string, object>)lfi.GetValue(last);
                    Dictionary<string, object> new_ld = new Dictionary<string, object>();

                    // Create another dictionary from dictionary
                    foreach (string key in ld.Keys)
                    {
                        if (ld[key] is string) new_ld.Add(key, "new");
                        if (ld[key] is DynamicJsonArray) new_ld.Add(key, new DynamicJsonArray(new object[0]));
                        if (ld[key] is DynamicJsonObject) new_ld.Add(key, new DynamicJsonObject(new Dictionary<string, object>()));
                    }

                    DynamicJsonObject jsonobj = new DynamicJsonObject(new_ld);
                    foos.Add(jsonobj);
                    fi.SetValue(item.obj, foos.ToArray());
                }
                else
                {
                    Dictionary<string, object> dict = new Dictionary<string, object>();
                    dict.Add("name", "new" + foos.Count + 1);
                    DynamicJsonObject jsonobj = new DynamicJsonObject(dict);                    
                    foos.Add(jsonobj);
                    fi.SetValue(item.obj, foos.ToArray());
                }
            }
        }

        private void FormatSource(dynamic inObj, TextEditorControl editor, bool asXml)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(inObj.Value, Newtonsoft.Json.Formatting.Indented) ;

            if (asXml)
            {
                string rootname = null;
                if (inObj is DynamicJsonArray)
                {
                    json = "{\"result\" : " + json + "}";
                    rootname = "result";
                }
                if (inObj is DynamicJsonObject)
                {
                    IEnumerable<string> propertyNames = inObj.GetDynamicMemberNames();
                    if (propertyNames.Count() > 1)
                    {
                        json = "{\"result\" : " + json + "}";
                        rootname = null;
                    }
                }
                if (inObj is string)
                {
                    json = "{\"result\" : " + json + "}";
                    rootname = "result";
                }
                json = Regex.Replace(json, @"("")(\w+)(""\s*)(:)(\s*"")", "$1@$2$3$4$5"); // Add attibutes
                var doc = JsonConvert.DeserializeXmlNode(json, rootname, true);
                if (doc == null) editor.Text = "<no results>"; 
                else editor.Text = PrintXML(doc);
                editor.Document.HighlightingStrategy = HighlightingStrategyFactory.CreateHighlightingStrategy("XML");
                editor.Document.FormattingStrategy = new XmlFormattingStrategy();
                editor.Document.FoldingManager.FoldingStrategy = new XmlFoldingStrategy();
                editor.Document.FoldingManager.UpdateFoldings(string.Empty, null);
                editor.Refresh();
            } else 
            {
                editor.Text = json;
                //editor.Document.HighlightingStrategy = new DefaultHighlightingStrategy();
                //editor.Document.FormattingStrategy = new DefaultFormattingStrategy();
                //editor.Document.FoldingManager.FoldingStrategy = new IndentFoldingStrategy();
                //editor.Document.FoldingManager.UpdateFoldings(string.Empty, null);
                editor.ActiveTextAreaControl.TextArea.Refresh(txtSource.ActiveTextAreaControl.TextArea.FoldMargin);
                editor.Refresh();
            }
        }

        private void SetPreview()
        {
            string path = statusLabel.Text;
            dynamic objQuery = srv.GetTree(path, false);

            FormatSource(new TNode() { Value = objQuery }, txtPreview, false);

            PropertyValue itemTree = (PropertyValue)tree.SelectedNode.Tag;
            FormatSource(new TNode() { Value = itemTree.obj }, txtSourceNode, false);

            // TNode tnode = srv.GetNode(path);
            // FormatSource(tnode, txtSourceNode, false);   
        }

        private void SetStatusBar()
        {

            //Navigate to the parent rows
            string path = "";
            TreeNode basenode = tree.SelectedNode;
            do
            {
                PropertyValue itemTree = (PropertyValue)basenode.Tag;

                if (!string.IsNullOrEmpty(path) && (!path.StartsWith("["))) path = "." + path;

                if ((itemTree.col != null))
                {
                    path = "[@" + itemTree.obj["name"] + "]" + path;
                }
                else
                {

                    path = itemTree.Property + path;
                }
                basenode = basenode.Parent;
            } while (basenode != null);

            statusLabel.Text = path;
            statusServer.Text = _serverurl;


        }

        private void Dirty()
        {
            statusPristine.Visible = false;
            statusDirty.Visible = true;
            statusError.Visible = false;
        }

        private void Pristine()
        {
            statusPristine.Visible = true;
            statusDirty.Visible = false;
            statusError.Visible = false;
        }

        private void Error(string message, string description)
        {
            statusPristine.Visible = false;
            statusDirty.Visible = false;
            statusError.Visible = true;
            ErrorForm frm = new ErrorForm();
            frm.Title = "Error";
            frm.Message = message;
            frm.Description = description;
            frm.ShowDialog(this);
            statusError.ToolTipText = message;
        }

        #endregion

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnGetNode_Click(object sender, EventArgs e)
        {
            dynamic objQuery = srv.GetNode(txtCommand.Text);
            FormatSource(objQuery, textEdQuery, rdQueryAsXml.Checked);
        }

        private void btnGetTree_Click(object sender, EventArgs e)
        {
            dynamic objQuery = srv.GetTree(txtCommand.Text, chObjectInfo.Checked);
            FormatSource(new TNode() { Value = objQuery }, textEdQuery, rdQueryAsXml.Checked);
        }

        private void grid2_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {

        }


        private void ExpandMyLitleBoys(TreeNode node, List<string> path)
        {
            if (path.Count == 0) return;
            path.RemoveAt(0);

            node.Expand();

            if (path.Count == 0)
                return;

            foreach (TreeNode mynode in node.Nodes)
            {
                if (path.Count == 0) continue;
                if (mynode.Text == path[0])
                {
                    ExpandMyLitleBoys(mynode, path); //recursive call
                    if (tree.SelectedNode.FullPath.Length < mynode.FullPath.Length) tree.SelectedNode = mynode;
                }
            }


        }

        private void btnSaveSourceNode_Click(object sender, EventArgs e)
        {
            try
            {
                string path = statusLabel.Text;

                var jsonContent = txtSourceNode.Text;

                TNode node = new TNode() { Path = path, Value = Json.Decode(jsonContent) };
                srv.SaveNode(node);

                // Reload the tree
                _obj = srv.GetNode();
                _obj.Value = Json.Decode(Newtonsoft.Json.JsonConvert.SerializeObject(_obj.Value));
                //_obj.Value = new DynamicJsonObject(_obj.Value);

                string nodepath = tree.SelectedNode.FullPath;

                tree.Populate(_obj.Value);

                var path_list = nodepath.Split('\\').ToList();
                foreach (TreeNode n in tree.Nodes)
                    if (n.Text == path_list[0])
                        ExpandMyLitleBoys(n, path_list);
                                

                //FormatSource(_obj, txtSource, rdAsXML.Checked);
                Pristine();
            }
            catch (Exception ex)
            {
                Error(ex.Message, ex.ToString());
            }
        }

        private void gridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            gridToolStripMenuItem.Checked = !gridToolStripMenuItem.Checked;
            panelTopGrid.Visible = splitterGrid.Visible = (gridToolStripMenuItem.Checked);
        }
    }
}
