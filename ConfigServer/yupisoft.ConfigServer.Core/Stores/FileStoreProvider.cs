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

        public event StoreChanged Change;

        public string FilePath { get; private set; }

        public string StartEntityName
        {
            get { return _entityName; }
            set
            {
                Regex rgx = new Regex("[^a-zA-Z0-9\\.]");
                _entityName = rgx.Replace(value, "");
            }
        }

        public IConfigWatcher Watcher { get{ return _watcher; } }

        public FileStoreProvider(string connectionString, string startEntityName, IConfigWatcher watcher, ILogger logger)
        {
            FilePath = connectionString;
            _entityName = startEntityName.Replace("/","\\");
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

            JToken token = JsonProcessor.Process(content, entityName, this);            
            return token;
        }

        public JToken GetRaw(string entityName)
        {
            entityName = entityName.Replace("/", "\\");
            string content = GetContent(entityName);
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
