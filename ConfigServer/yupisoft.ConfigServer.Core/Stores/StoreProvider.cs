using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace yupisoft.ConfigServer.Core.Stores
{
    public interface IStoreProvider 
    {        
        void Initialize(string connectionString);
        /// <summary>
        /// Get the content by connection parameters
        /// </summary>
        /// <param name="connectionString">Represent the way to connect to the source</param>
        /// <param name="baseSource">The entry point of the data</param>
        /// <returns></returns>
        TNode Get(string baseSource);
        void Set(TNode token);
    }
}
