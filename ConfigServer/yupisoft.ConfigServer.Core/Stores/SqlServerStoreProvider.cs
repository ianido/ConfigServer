using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using yupisoft.ConfigServer.Core.Json;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace yupisoft.ConfigServer.Core.Stores
{
    public class SqlServerStoreProvider : IStoreProvider
    {
        private IConfigWatcher _watcher;
        private string _entityName;

        public event StoreChanged Change;

        public string ConnectionString { get; private set; }
        public string StartEntityName
        {
            get
            {
                return _entityName;
            }
            set
            {
                Regex rgx = new Regex("[^a-zA-Z0-9]");
                _entityName = rgx.Replace(value, "");
            }
        }
        public SqlServerStoreProvider(string connectionString, string startEntityName, IConfigWatcher watcher)
        {
            ConnectionString = connectionString;
            _entityName = startEntityName;
            _watcher = watcher;
    }
        /// <summary>
        /// Node Path should include the UPDATE QUERY with @node parameter: UPDATE Table1 set node = @node where key = key1
        /// </summary>
        /// <param name="node"></param>
        public void Set(JToken node, string entityName)
        {            
            SqlConnection conn = new SqlConnection(ConnectionString);
            SqlCommand cmdExist = new SqlCommand("SELECT count(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @entity", conn);
            cmdExist.Parameters.AddWithValue("@entity", entityName);
            SqlCommand cmdCreate = new SqlCommand("CREATE TABLE " + entityName + " (id int IDENTITY(1,1) PRIMARY KEY, node NVARCHAR(MAX), created datetime)", conn);
            SqlCommand cmd = new SqlCommand("insert into " + entityName + " (node, created) values (@node, @created) ", conn);
            cmd.Parameters.AddWithValue("@node", JsonConvert.SerializeObject(node));
            cmd.Parameters.AddWithValue("@created", DateTime.UtcNow);
            try
            {
                conn.Open();
                if ((int)cmdExist.ExecuteScalar() == 0)
                    cmdCreate.ExecuteNonQuery();
                cmd.ExecuteNonQuery();
            }
            finally
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
            }
        }
        /// <summary>
        /// Connect to the proper database and return the base node processed.
        /// </summary>
        /// <param name="connectionString">connection String to SQL Server database</param>
        /// <param name="baseSource">Singled Result Query to obtain the basenode; SELECT node From Table where key1 = key</param>
        /// <returns></returns>
        public JToken Get(string entityName)
        {
            SqlConnection conn = new SqlConnection(ConnectionString);
            SqlCommand cmd = new SqlCommand("select top 1 node from " + entityName + " orderby created desc", conn);
            try
            {
                conn.Open();
                var content = (string)cmd.ExecuteScalar();
                JToken token = JsonProcessor.Process(content, this, _watcher);
                return token;
            }
            finally
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
            }
        }
    }
}
