using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using yupisoft.ConfigServer.Core.Json;
using Newtonsoft.Json;

namespace yupisoft.ConfigServer.Core.Stores
{
    public class FileStoreProvider : IStoreProvider
    {
        public string FilePath { get; private set; }
        public string FileName { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString">It is the path to the folder that contain the configuration files</param>
        /// <param name="baseSource">The entry point file name. (main file)</param>
        /// <returns></returns>
        public JToken Get()
        {
            string content = System.IO.File.ReadAllText(FilePath);            
            JToken token = JsonProcessor.Process(content);
            return token;
        }

        public void Initialize(string connectionString, string getCommand, string saveCommand)
        {
            FilePath = connectionString;
            FileName = saveCommand;
        }

        public void Set(JToken node)
        {
            string content = JsonConvert.SerializeObject(node);
            System.IO.File.WriteAllText(FilePath, content);
        }
    }
}
