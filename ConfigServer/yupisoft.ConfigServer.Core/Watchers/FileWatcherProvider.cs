using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public string EntityName { get { return _entityName; } set { _entityName = value; } }

        public bool EnableRaisingEvents { get { return _enableRaisingEvents; } set { _enableRaisingEvents = value; } }

        public DateTime LastWriteDate { get { return _lastWriteDate; } set { _lastWriteDate = value; } }

        public event EntityChangeEventHandler Changed;

        public void CheckForChange() {
            FileInfo fi = new FileInfo(EntityName);
            if (fi.LastWriteTimeUtc != LastWriteDate)
            {
                EnableRaisingEvents = false;
                Changed(this, fi.FullName);
                LastWriteDate = fi.LastWriteTimeUtc;
                EnableRaisingEvents = true;
            }
        }        
    }
}
