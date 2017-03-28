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

        private ConfigServerTenants _tenants;

        public ConfigServerManager(ConfigServerTenants tenants, ILogger<ConfigServerManager> logger)
        {
            _tenants = tenants;
            _logger = logger;
            foreach (var tenant in _tenants.Tenants)
            {
                tenant.Store.Change += Store_Change;
                tenant.Load(true);
            }
            _logger.LogTrace("Created ConfigManager with " + _tenants.Tenants.Count + " tenants.");
        }

        private void Store_Change(IStoreProvider sender, string entityName)
        {
            foreach (var tenant in _tenants.Tenants)
            {
                if (tenant.Store.StartEntityName == sender.StartEntityName)
                    tenant.Load(false);
            }
        }

        private ConfigServerTenant GetTenant(int tenantId)
        {
            foreach (var tenant in _tenants.Tenants)
            {
                if (tenant.TenantConfig.Id == tenantId)
                    return tenant;
            }
            return null;
        }

        public TNode GetRaw(string path, string entityName, int tenantId)
        {
            var tenant = GetTenant(tenantId);
            if (tenant == null) throw new Exception("Tenant: " + tenantId + " not found.");
            if (tenant.Token == null) throw new Exception("Tenant: " + tenantId + " not loaded.");
             
            lock (tenant.Token)
            {
                if (entityName == "@default") entityName = tenant.Store.StartEntityName;
                JToken selToken = tenant.RawTokens[entityName].SelectToken(path);
                if (selToken == null) return new TNode(path, "{}", entityName);
                var result = selToken.ToObject<JToken>();
                return new TNode(path, result, entityName);
            }
        }

        public JToken Get(string path, int tenantId)
        {
            return Get<JToken>(path, tenantId);
        }

        public T Get<T>(string path, int tenantId)
        {
            var tenant = GetTenant(tenantId);
            if (tenant == null) throw new Exception("Tenant: " + tenantId + " not found.");
            if (tenant.Token == null) throw new Exception("Tenant: " + tenantId + " not loaded.");

            lock (tenant.Token)
            {
                JToken selToken = tenant.Token.SelectToken(path);
                if (selToken == null) return default(T);
                var result = selToken.ToObject<T>();
                return result;
            }
        }

        public bool Set(TNode newToken, int tenantId)
        {
            var tenant = GetTenant(tenantId);
            if (tenant == null) throw new Exception("Tenant: " + tenantId + " not found.");
            if (tenant.Token == null) throw new Exception("Tenant: " + tenantId + " not loaded.");

            lock (tenant.Token)
            {
                if (tenant.Store.Watcher.IsWatching(newToken.Entity))
                {
                    JToken rawToken = tenant.Store.GetRaw(newToken.Entity);
                    JToken selToken = rawToken.SelectToken(newToken.Path);
                    if (selToken == null) return false;
                    selToken.Replace(newToken.Value);
                    tenant.Store.Set(rawToken, newToken.Entity);
                }
                else
                    throw new Exception("Unauthorized Entity: "+ newToken.Entity);
            }
            return true;
        }
    }
}
