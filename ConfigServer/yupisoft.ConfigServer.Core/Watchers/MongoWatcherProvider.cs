using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MongoDB.Bson;
using static yupisoft.ConfigServer.Core.Stores.MongoStoreProvider;

namespace yupisoft.ConfigServer.Core.Watchers
{

    public class MongoWatcherProvider : IWatcherProvider
    {

        private string _connection;
        private string _dbmongo;
        private string _entityName;
        private bool _enableRaisingEvents;
        private DateTime _lastWriteDate;

        public string Connection { get { return _connection; }
            set {
                string[] connectionStringParts = value.Split('|');
                if (connectionStringParts.Length < 2) throw new Exception("Incorrect Connection String");
                _connection = connectionStringParts[0];
                _dbmongo = connectionStringParts[1];
            }
        }

        public string EntityName
        {
            get { return _entityName; }
            set
            {
                Regex rgx = new Regex("[^a-zA-Z0-9\\.]");
                _entityName = rgx.Replace(value, "");
            }
        }

        public bool EnableRaisingEvents { get { return _enableRaisingEvents; } set { _enableRaisingEvents = value; } }

        public DateTime LastWriteDate { get { return _lastWriteDate; } set { _lastWriteDate = value; } }

        public event EntityChangeEventHandler Changed;

        public void CheckForChange()
        {
            if (!EnableRaisingEvents) return;
            var _client = new MongoClient(_connection);
            var _db = _client.GetDatabase(_dbmongo);
            if (_db == null) throw new Exception("No Database named: " + _dbmongo);

            var collection = _db.GetCollection<DBRecord>(EntityName);
            if (collection == null) return;

            var v = collection.Find("{}").SortByDescending(p=>p.Created).Limit(1);
            var LastWriteTimeUtc = v.FirstOrDefault()?.Created;

            if (LastWriteTimeUtc != LastWriteDate)
            {
                EnableRaisingEvents = false;
                Changed(this, EntityName);
                LastWriteDate = LastWriteTimeUtc??DateTime.MinValue;
                EnableRaisingEvents = true;
            }           
        }

        public void RestartObservationDate()
        {
            var _client = new MongoClient(_connection);
            var _db = _client.GetDatabase(_dbmongo);
            if (_db == null) throw new Exception("No Database named: " + _dbmongo);

            var collection = _db.GetCollection<DBRecord>(EntityName);
            if (collection == null) return;

            var v = collection.Find("{}").SortByDescending(p => p.Created).Limit(1);
            var LastWriteTimeUtc = v.FirstOrDefault()?.Created;

            LastWriteDate = LastWriteTimeUtc??DateTime.MinValue;            
        }
    }
}
