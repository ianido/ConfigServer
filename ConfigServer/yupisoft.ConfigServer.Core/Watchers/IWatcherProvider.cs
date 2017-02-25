using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace yupisoft.ConfigServer.Core
{
    public delegate void EntityChangeEventHandler(object sender, string entityName);

    public interface IWatcherProvider 
    {
        string Connection { get; set; }

        string EntityName { get; set; }

        bool EnableRaisingEvents { get; set; }

        DateTime LastWriteDate { get; set; }

        event EntityChangeEventHandler Changed;

        void CheckForChange();        
    }
}
