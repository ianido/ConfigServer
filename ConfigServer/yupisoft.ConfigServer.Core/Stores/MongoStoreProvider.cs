using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using yupisoft.ConfigServer.Core.Json;

namespace yupisoft.ConfigServer.Core.Stores
{
    public class MongoStoreProvider : IStoreProvider
    {
        public string MongoConnection { get; private set; }
        public string MongoDatabase { get; private set; }
        public string GetCommand { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString">the connection String to the mongo DB Server|database: "mongodb://localhost:27017|EmployeeDB"</param>
        /// <param name="getCommand">tha query to the collection which contain the actual document node: "Collection Name|filter"  filter should be like {key: value}</param>
        ///                                                            will be equivalent to:             select * from Collection Where Key = Value
        /// <param name="saveCommand"></param>
        public void Initialize(string connectionString, string getCommand, string saveCommand)
        {
            string[] connectionStringParts = connectionString.Split('|');
            GetCommand = getCommand;
            if (connectionStringParts.Length < 2) throw new Exception("Incorrect Connection String");
            MongoConnection = connectionStringParts[0];
            MongoDatabase = connectionStringParts[1];
        }

        public void Set(JToken node)
        {
            throw new NotImplementedException();
        }

        public JToken Get()
        {
            string[] commandStringParts = GetCommand.Split('|');
            if (commandStringParts.Length < 2) throw new Exception("Incorrect Query String");

            var _client = new MongoClient(MongoConnection);
            var _db = _client.GetDatabase(MongoDatabase);

            var collection = _db.GetCollection<BsonDocument>(commandStringParts[0]);
            FilterDefinition<BsonDocument> filter = commandStringParts[1]; // "{ x: 1 }";

            var v = collection.Find(filter);
            var content = v.ToJson();
            var token = JsonProcessor.Process(content);
            return token;
        }
    }
}
