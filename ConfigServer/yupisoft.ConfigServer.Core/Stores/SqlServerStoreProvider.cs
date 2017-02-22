using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using yupisoft.ConfigServer.Core.Json;
using Newtonsoft.Json;

namespace yupisoft.ConfigServer.Core.Stores
{
    public class SqlServerStoreProvider : IStoreProvider
    {
        public string ConnectionString { get; private set; }
        public string GetCommand { get; private set; }
        public string SaveCommand { get; private set; }

        public void Initialize(string connectionString, string getCommand, string saveCommand)
        {
            ConnectionString = connectionString;
            GetCommand = getCommand;
            SaveCommand = saveCommand;
        }

        /// <summary>
        /// Node Path should include the UPDATE QUERY with @node parameter: UPDATE Table1 set node = @node where key = key1
        /// </summary>
        /// <param name="node"></param>
        public void Set(JToken node)
        {
            SqlConnection conn = new SqlConnection(ConnectionString);
            SqlCommand cmd = new SqlCommand(SaveCommand, conn);
            cmd.Parameters.AddWithValue("@node", JsonConvert.SerializeObject(node));
            try
            {
                var content = (string)cmd.ExecuteScalar();               
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
        public JToken Get()
        {
            SqlConnection conn = new SqlConnection(ConnectionString);
            SqlCommand cmd = new SqlCommand(GetCommand, conn);
            try
            {
                var content = (string)cmd.ExecuteScalar();
                JToken token = JsonProcessor.Process(content);
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
