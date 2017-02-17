using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using yupisoft.ConfigServer.Core.Json;

namespace yupisoft.ConfigServer.Core.Stores
{
    public class SqlServerStoreProvider : IStoreProvider
    {
        public string ConnectionString { get; private set; }

        public void Initialize(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public void Set(TNode node)
        {
            SqlConnection conn = new SqlConnection(ConnectionString);
            SqlCommand cmd = new SqlCommand(node.Path, conn);
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
        /// <param name="baseSource">Query to obtain the basenode</param>
        /// <returns></returns>
        public TNode Get(string baseSource)
        {
            SqlConnection conn = new SqlConnection(ConnectionString);
            SqlCommand cmd = new SqlCommand(baseSource, conn);
            try
            {
                var content = (string)cmd.ExecuteScalar();
                JToken token = JsonProcessor.Process(content);
                return new TNode(baseSource, token);
            }
            finally
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
            }
        }
    }
}
