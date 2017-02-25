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

        private ConfigServerTenants _tenants;


        public ConfigServerManager(ConfigServerTenants tenants)
        {
            _tenants = tenants;
            foreach (var tenant in _tenants.Tenants)
            {
                tenant.Store.Change += _store_Change;
                tenant.Token = tenant.Store.Get(tenant.Store.StartEntityName);
            }
        }

        private void _store_Change(IStoreProvider sender, JToken newToken)
        {
            foreach (var tenant in _tenants.Tenants)
            {
                if (tenant.Store.StartEntityName == sender.StartEntityName)
                    tenant.Token = newToken;
            }
        }

        public JToken Get(string path, int tenantId)
        {
            return Get<JToken>(path, tenantId);
        }

        public T Get<T>(string path, int tenantId)
        {
            JToken token = null;
            bool found = false;

            foreach (var tenant in _tenants.Tenants)
            {
                if (tenant.TenantConfig.Id == tenantId)
                {
                    token = tenant.Token;
                    found = true;
                }
            }

            if (!found) throw new Exception("Tenant: " + tenantId + " not found.");
            if (token == null) throw new Exception("Tenant: " + tenantId + " not loaded.");

            lock (token)
            {
                JToken selToken = token.SelectToken(path);
                if (selToken == null) return default(T);
                var result = selToken.ToObject<T>();
                return result;
            }
        }

        public bool Set(string path, int tenantId, JToken newToken)
        {
            JToken token = null;
            IStoreProvider store = null;
            bool found = false;


            foreach (var tenant in _tenants.Tenants)
            {
                if (tenant.TenantConfig.Id == tenantId)
                {
                    token = tenant.Token;
                    store = tenant.Store;
                    found = true;
                }
            }
            if (!found) throw new Exception("Tenant: " + tenantId + " not found.");
            if (token == null) throw new Exception("Tenant: " + tenantId + " not loaded.");

            lock (token)
            {
                JToken selToken = token.SelectToken(path);
                if (selToken == null) return false;
                selToken.Replace(newToken);
                store.Set(token, store.StartEntityName); //Save Modified Store
            }
            return true;
        }
    }
}
