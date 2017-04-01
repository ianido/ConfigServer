using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using yupisoft.ConfigServer.Core.Json;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MongoDB.Bson.Serialization;
using System.Text.RegularExpressions;

namespace yupisoft.ConfigServer.Core.Stores
{
    public class MongoStoreProvider : IStoreProvider
    {
        public class DBRecord
        {
            public ObjectId Id { get; set; }
            public string Content { get; set; }
            public DateTime Created { get; set; }
        }

        private IConfigWatcher _watcher;
        private ILogger _logger;

        private string _entityName;

        private string GetContent(string entityName)
        {
            var _client = new MongoClient(MongoConnection);
            var _db = _client.GetDatabase(MongoDatabase);
            var collection = _db.GetCollection<DBRecord>(entityName);
            var v = collection.Find("{}").SortByDescending(r => r.Created).Limit(1);

            DBRecord obj = null;

            if (v.Count() > 0)
            {
                obj = v.FirstOrDefault<DBRecord>();
            }
            else
            {
                obj = new DBRecord();
                obj.Id = ObjectId.GenerateNewId();
                obj.Created = DateTime.UtcNow;
                obj.Content = "{}";
                collection.InsertOne(obj);
            }
            _watcher.AddToWatcher(entityName, MongoConnection + "|" + MongoDatabase, obj.Created);
            return obj.Content;
        }

        public event StoreChanged Change;

        public IConfigWatcher Watcher { get { return _watcher; } }

        public string MongoConnection { get; private set; }

        public string MongoDatabase { get; private set; }

        public string StartEntityName
        {
            get { return _entityName; }
            set
            {
                Regex rgx = new Regex("[^a-zA-Z0-9\\.]");
                _entityName = rgx.Replace(value, "");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString">the connection String to the mongo DB Server|database: "mongodb://localhost:27017|EmployeeDB"</param>
        public MongoStoreProvider(string connectionString, string startEntityName, IConfigWatcher watcher, ILogger logger)
        {
            string[] connectionStringParts = connectionString.Split('|');
            if (connectionStringParts.Length < 2) throw new Exception("Incorrect Connection String");
            MongoConnection = connectionStringParts[0];
            MongoDatabase = connectionStringParts[1];
            _entityName = startEntityName;
            _watcher = watcher;
            _watcher.Change += _watcher_Change;
            _logger = logger;
        }

        private void _watcher_Change(object sender, string fileName)
        {
            Change(this, fileName);
        }

        public void Set(JToken node, string entityName)
        {
            var _client = new MongoClient(MongoConnection);
            var _db = _client.GetDatabase(MongoDatabase);
            var collection = _db.GetCollection<DBRecord>(entityName);
            DBRecord obj = new DBRecord();
            obj.Id = ObjectId.GenerateNewId();
            obj.Created = DateTime.UtcNow;
            obj.Content = node.ToString(Formatting.Indented);
            collection.InsertOne(obj);
        }

        public JToken Get(string entityName)
        {
            var content = GetContent(entityName);
            var token = JsonProcessor.Process(content, entityName, this);
            return token;
        }

        public JToken GetRaw(string entityName)
        {
            var content = GetContent(entityName);
            var token =  JsonConvert.DeserializeObject<JToken>(content);
            return token;
        }
    }
}
