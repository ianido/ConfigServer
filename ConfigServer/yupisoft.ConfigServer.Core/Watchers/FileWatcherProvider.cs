using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace yupisoft.ConfigServer.Core.Watchers
{

    public class FileWatcherProvider : IWatcherProvider
    {
        private string _connection;
        private string _entityName;
        private bool _enableRaisingEvents;
        private DateTime _lastWriteDate;

        public string Connection { get { return _connection; } set { _connection = value; } }

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

        public void CheckForChange() {
            if (!EnableRaisingEvents) return;
            if (!File.Exists(Path.Combine(_connection, EntityName)))
                return;            
            FileInfo fi = new FileInfo(Path.Combine(_connection,EntityName));
            if (fi.LastWriteTimeUtc != LastWriteDate)
            {
                EnableRaisingEvents = false;
                Changed(this, fi.FullName);
                LastWriteDate = fi.LastWriteTimeUtc;
                EnableRaisingEvents = true;
            }
        }

        public void RestartObservationDate()
        {
            FileInfo fi = new FileInfo(Path.Combine(_connection, EntityName));
            LastWriteDate = fi.LastWriteTimeUtc;
        }
    }
}
