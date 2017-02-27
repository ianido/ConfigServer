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
            get
            {
                return _entityName;
            }
        }
        public FileStoreProvider(string connectionString, string startEntityName, IConfigWatcher watcher, ILogger logger)
        {
            FilePath = connectionString;
            _entityName = startEntityName;
            _logger = logger;
            _watcher = watcher;
            _watcher.Change += _watcher_Change;
        }
        private void _watcher_Change(object sender, string fileName)
        {
            var token = Get(StartEntityName);
            Change(this, token);
        }
        public JToken Get(string entityName)
        {            
            string[] filesInFolder = Directory.GetFiles(FilePath, Path.GetFileNameWithoutExtension(entityName) + "_*" + Path.GetExtension(entityName));
            if (filesInFolder.Length == 0)
                filesInFolder = Directory.GetFiles(FilePath, Path.GetFileName(entityName));
            Dictionary<string, DateTime> arr = new Dictionary<string, DateTime>();
            if (filesInFolder.Length == 0) return null; 

            foreach (var file in filesInFolder)
            {
                string[] fileParts = Path.GetFileNameWithoutExtension(file).Split('_');
                var date = (fileParts.Length > 1) ? fileParts[1] : new FileInfo(file).LastWriteTimeUtc.ToString(FILEDATEFORMAT);
                arr.Add(file, DateTime.ParseExact(date, FILEDATEFORMAT, null));
            }

            arr.OrderByDescending(e => e.Value);
            var mostRecent = arr.Last();

            string content = "";
            lock (FilePath)
            {
                string fullFilePath = mostRecent.Key;
                content = File.ReadAllText(fullFilePath);
                _watcher.AddToWatcher(fullFilePath);
            }          
            JToken token = JsonProcessor.Process(content, this, _watcher);
            
            return token;
        }
        public void Set(JToken node, string entityName)
        {
            string content = JsonConvert.SerializeObject(node);
            /*
             * Estrategia de Saving Node

1 - Get Raw Node and Save Entity as Is
2 - Get Calculated Node And When I attempt to Save:
  - Get Node Map
  - For every Node I have: EntityName, OriginalValue

             */
            lock (FilePath)
            {
                string filePath = Path.Combine(FilePath, Path.GetFileNameWithoutExtension(entityName) + "_" + DateTime.UtcNow.ToString(FILEDATEFORMAT) + Path.GetExtension(entityName));
                File.WriteAllText(filePath, content);
                _watcher.ClearWatcher();
            }
            Get(entityName);
        }
    }
}
