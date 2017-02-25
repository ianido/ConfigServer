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
        private IConfigWatcher _watcher;
        private string _entityName;

        public event StoreChanged Change;

        public string MongoConnection { get; private set; }
        public string MongoDatabase { get; private set; }
        public string StartEntityName
        {
            get
            {
                return _entityName;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString">the connection String to the mongo DB Server|database: "mongodb://localhost:27017|EmployeeDB"</param>
        public MongoStoreProvider(string connectionString, string startEntityName, IConfigWatcher watcher)
        {
            string[] connectionStringParts = connectionString.Split('|');
            if (connectionStringParts.Length < 2) throw new Exception("Incorrect Connection String");
            MongoConnection = connectionStringParts[0];
            MongoDatabase = connectionStringParts[1];
            _entityName = startEntityName;
            _watcher = watcher;
    }
        public void Set(JToken node, string entityName)
        {
            var _client = new MongoClient(MongoConnection);            
            var _db = _client.GetDatabase(MongoDatabase);
            if (_db == null) throw new Exception("No Database named: " + MongoDatabase);
            var collection = _db.GetCollection<BsonDocument>(entityName);
            if (collection == null)
            {
                // Create Collection if not exist
                _db.CreateCollection(entityName);
                collection = _db.GetCollection<BsonDocument>(entityName);
            }
            JObject obj = new JObject();
            obj["created"] = DateTime.UtcNow;
            obj["node"] = node;
            var toInsert = obj.ToBsonDocument(); 
            collection.InsertOne(toInsert);
        }
        public JToken Get(string entityName)
        {
            var _client = new MongoClient(MongoConnection);
            var _db = _client.GetDatabase(MongoDatabase);
            if (_db == null) throw new Exception("No Database named: " + MongoDatabase);
            var collection = _db.GetCollection<BsonDocument>(entityName);
            if (collection == null) return null;
            var v = collection.Find("{}").Sort("{created:-1}").Limit(1);
            var content = v.First()["node"].ToJson();
            if (content == null) content = "{}";
            var token = JsonProcessor.Process(content, this, _watcher);
            return token;
        }
    }
}
