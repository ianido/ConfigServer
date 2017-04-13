using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using yupisoft.ConfigServer.Core.Json;
using Newtonsoft.Json;
using System.IO;
using System.Collections;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace yupisoft.ConfigServer.Core.Stores
{
    public class FileStoreProvider : IStoreProvider
    {
        private IConfigWatcher _watcher;
        private ILogger _logger;
        private string FILEDATEFORMAT = "yyyy-MM-dd-hh-mm-ss";
        private string _entityName;
        private string _aclName;

        public event StoreChanged Change;

        public string FilePath { get; private set; }

        public string StartEntityName
        {
            get { return _entityName; }
            set
            {
                if (string.IsNullOrEmpty(value)) _entityName = value;
                else
                {
                    Regex rgx = new Regex("[^a-zA-Z0-9\\.]");
                    _entityName = rgx.Replace(value, "");
                }
            }
        }
        public string ACLEntityName
        {
            get { return _aclName; }
            set
            {
                Regex rgx = new Regex("[^a-zA-Z0-9\\.]");
                _aclName = rgx.Replace(value, "");
            }
        }

        public IConfigWatcher Watcher { get{ return _watcher; } }

        public FileStoreProvider(StoreConfigSection config, IConfigWatcher watcher, ILogger logger)
        {
            FilePath = config.Connection;
            _entityName = config.StartEntityName.Replace("/","\\");
            _aclName = config.ACLEntityName?.Replace("/", "\\");
            _logger = logger;
            _watcher = watcher;
            _watcher.Change += _watcher_Change;
        }

        private void _watcher_Change(object sender, string fileName)
        {
            Change(this, fileName);
        }

        private string GetContent(string entityName)
        {
            entityName = entityName.Replace("/", "\\");
            lock (FilePath)
            {
                var fullFilePath = Path.Combine(FilePath, entityName);
                var content = File.ReadAllText(fullFilePath);

                _watcher.AddToWatcher(Path.GetFileName(fullFilePath), Path.GetDirectoryName(fullFilePath), new FileInfo(fullFilePath).LastWriteTimeUtc);
                return content;
            }            
        }

        public JToken Get(string entityName)
        {
            entityName = entityName.Replace("/", "\\");
            string content = GetContent(entityName);
            _logger.LogTrace("Retrieved: " + entityName);
            JToken token = JsonProcessor.Process(content, entityName, this);            
            return token;
        }

        public JToken GetRaw(string entityName)
        {
            entityName = entityName.Replace("/", "\\");
            string content = GetContent(entityName);
            _logger.LogTrace("Retrieved Raw: " + entityName);
            return JsonConvert.DeserializeObject<JToken>(content);
        }

        public void Set(JToken node, string entityName)
        {
            entityName = entityName.Replace("/", "\\");
            string content = JsonConvert.SerializeObject(node, Formatting.Indented);
            lock (FilePath)
            {
                var fullFilePath = Path.Combine(FilePath, entityName);                
                string bkpfilePath = Path.Combine(FilePath, Path.GetFileNameWithoutExtension(entityName) + "_" + DateTime.UtcNow.ToString(FILEDATEFORMAT) + Path.GetExtension(entityName));
                File.Copy(fullFilePath, bkpfilePath);
                File.WriteAllText(fullFilePath, content);
            }
        }
    }
}
