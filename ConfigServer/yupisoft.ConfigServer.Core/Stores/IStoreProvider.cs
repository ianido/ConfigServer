using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace yupisoft.ConfigServer.Core
{
    public delegate void StoreChanged(IStoreProvider sender, JToken newToken);
    public interface IStoreProvider 
    {
        event StoreChanged Change;
        string StartEntityName { get; }
        JToken Get(string entityName);
        void Set(JToken node, string entityName);
    }
}
