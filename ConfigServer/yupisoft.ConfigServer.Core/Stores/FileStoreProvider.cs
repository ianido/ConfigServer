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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString">It is the path to the folder that contain the configuration files</param>
        /// <param name="baseSource">The entry point file name. (main file)</param>
        /// <returns></returns>
        public TNode Get(string baseSource)
        {
            string content = System.IO.File.ReadAllText(FilePath);            
            JToken token = JsonProcessor.Process(content);
            return new TNode(baseSource, token);
        }

        public void Initialize(string connectionString)
        {
            FilePath = connectionString;
        }

        public void Set(TNode node)
        {
            throw new NotImplementedException();
        }
    }
}
