using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace yupisoft.ConfigServer.Core.Watchers
{

    public class FileWatcherProvider : IWatcherProvider
    {
        private string _connection;
        private string _entityName;
        private bool _enableRaisingEvents;
        private DateTime _lastWriteDate;
        private string CheckSum;

        public string Connection { get { return _connection; } set { _connection = value; } }

        public string EntityName { get { return _entityName; } set { _entityName = value; } }

        public bool EnableRaisingEvents { get { return _enableRaisingEvents; } set { _enableRaisingEvents = value; } }

        public DateTime LastWriteDate { get { return _lastWriteDate; } set { _lastWriteDate = value; } }

        public event EntityChangeEventHandler Changed;

        public void CheckForChange() {
            if (!EnableRaisingEvents) return;
            FileInfo fi = new FileInfo(Path.Combine(_connection,EntityName));
            if (fi.LastWriteTimeUtc != LastWriteDate)
            {
                EnableRaisingEvents = false;
                Changed(this, fi.FullName);
                LastWriteDate = fi.LastWriteTimeUtc;
                EnableRaisingEvents = true;
            }
        }

        private static string GetChecksum(string file)
        {
            using (FileStream stream = File.OpenRead(file))
            {
                var sha = new HMACSHA256();
                byte[] checksum = sha.ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", String.Empty);
            }
        }

        public void RestartObservationDate()
        {
            FileInfo fi = new FileInfo(Path.Combine(_connection, EntityName));
            LastWriteDate = fi.LastWriteTimeUtc;
        }
    }
}
