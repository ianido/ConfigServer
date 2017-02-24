using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace yupisoft.ConfigServer.Core
{
    public class ConfigServerManager
    {
        private static object objlock = new object();

        private ILogger _logger;
        private IStoreProvider _store;
        private JToken _token;

        public ConfigServerManager(IStoreProvider store, ILogger<ConfigServerManager> logger)
        {
            _logger = logger;
            _store = store;
            _store.Change += _store_Change;
            _token = _store.Get(_store.StartEntityName);            
        }

        private void _store_Change(JToken newToken)
        {
            _token = newToken;
        }

        public JToken Get(string path)
        {
            return Get<JToken>(path);
        }

        public T Get<T>(string path)
        {
            lock (objlock)
            {
                JToken selToken = _token.SelectToken(path);
                if (selToken == null) return default(T);
                var result = selToken.ToObject<T>();
                return result;
            }
        }

        public bool Set(string path, JToken token)
        {
            lock (objlock)
            {
                JToken selToken = token.SelectToken(path);
                if (selToken == null) return false;
                selToken.Replace(token);
                _store.Set(token, _store.StartEntityName); //Save Modified Store
            }
            return true;
        }

    }
}
