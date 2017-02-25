using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace yupisoft.ConfigServer.Core.Watchers
{

    public class SqlServerWatcherProvider : IWatcherProvider
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
                Regex rgx = new Regex("[^a-zA-Z0-9]");
                _entityName = rgx.Replace(value, "");
            }
        }

        public bool EnableRaisingEvents { get { return _enableRaisingEvents; } set { _enableRaisingEvents = value; } }

        public DateTime LastWriteDate { get { return _lastWriteDate; } set { _lastWriteDate = value; } }

        public event EntityChangeEventHandler Changed;

        public void CheckForChange()
        {
            SqlConnection conn = new SqlConnection(Connection);
            SqlCommand cmd = new SqlCommand("select top 1 created from " + EntityName + " orderby created desc", conn);
            try
            {
                conn.Open();
                var lastDate = (DateTime)cmd.ExecuteScalar();
                if (lastDate != LastWriteDate)
                {
                    EnableRaisingEvents = false;
                    Changed(this, EntityName);
                    LastWriteDate = lastDate;
                    EnableRaisingEvents = true;
                }
            }
            finally
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
            }
        }
    }
}
