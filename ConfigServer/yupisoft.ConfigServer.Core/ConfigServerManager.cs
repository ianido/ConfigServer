using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using yupisoft.ConfigServer.Core.Cluster;

namespace yupisoft.ConfigServer.Core
{
    public delegate void DataChangedEventHandler(int tenantId, string entity, JToken jsonDiff);

    public class ConfigServerManager
    {
        private static object objlock = new object();

        private ILogger _logger;

        public ConfigServerTenants TenantManager { get; set; }

        public DateTime AliveSince { get; private set; }

        public event DataChangedEventHandler DataChanged;

        public ConfigServerManager(ConfigServerTenants tenants, ILogger<ConfigServerManager> logger)
        {
            TenantManager = tenants;
            _logger = logger;
            AliveSince = DateTime.UtcNow;

            foreach (var tenant in TenantManager.Tenants)
            {
                tenant.Store.Change += Store_Change;
                tenant.Load(true);
            }
            _logger.LogTrace("Created ConfigManager with " + TenantManager.Tenants.Count + " tenants.");            
        }

        private void Store_Change(IStoreProvider sender, string entityName)
        {
            foreach (var tenant in TenantManager.Tenants)
            {
                if (tenant.Store.StartEntityName == sender.StartEntityName)
                {
                    var loadResult = tenant.Load(false);
                    if (loadResult.Changes.Length > 0)
                        foreach (var e in loadResult.Changes)
                            DataChanged?.Invoke(tenant.TenantConfig.Id, e.entity, e.diffToken);
                }
            }

            
        }

        private ConfigServerTenant GetTenant(int tenantId)
        {
            foreach (var tenant in TenantManager.Tenants)
            {
                if (tenant.TenantConfig.Id == tenantId)
                    return tenant;
            }
            return null;
        }

        public JNode GetRaw(string path, string entityName, int tenantId)
        {
            var tenant = GetTenant(tenantId);
            if (tenant == null) throw new Exception("Tenant: " + tenantId + " not found.");
            if (tenant.Token == null) throw new Exception("Tenant: " + tenantId + " not loaded.");
             
            lock (tenant.Token)
            {
                if (entityName == "@default") entityName = tenant.Store.StartEntityName;
                JToken selToken = tenant.RawTokens[entityName].SelectToken(path);
                if (selToken == null) return new JNode(path, "{}", entityName);
                var result = selToken.ToObject<JToken>();
                return new JNode(path, result, entityName);
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

        public bool Set(JNode newToken, int tenantId)
        {
            return Set(newToken, tenantId, false);
        }

        internal bool Set(JNode newToken, int tenantId, bool stopMonitoring)
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
                    if (selToken.Parent != null)
                        selToken.Replace(newToken.Value);
                    else
                        rawToken = newToken.Value;
                    if (stopMonitoring) tenant.Store.Watcher.StopMonitoring();
                    tenant.Store.Set(rawToken, newToken.Entity);
                    if (stopMonitoring)
                    {
                        var res = tenant.Load(false);
                        if (res.Changes.Length > 0)
                        {
                            _logger.LogTrace("Applied Update: ");
                            foreach(var c in res.Changes)
                                _logger.LogTrace(c.diffToken?.ToString(Newtonsoft.Json.Formatting.Indented));
                        }
                    }
                }
                else
                    throw new Exception("Unauthorized Entity: " + newToken.Entity);
            }
            return true;
        }

        public bool ApplyUpdate(int tenantId, string entity, string jsonDiff)
        {
            if (string.IsNullOrEmpty(jsonDiff)) return true;
            var tenant = GetTenant(tenantId);
            if (tenant == null) throw new Exception("Tenant: " + tenantId + " not found.");
            if (tenant.Token == null) throw new Exception("Tenant: " + tenantId + " not loaded.");

            lock (tenant.Token)
            {
                if (tenant.Store.Watcher.IsWatching(entity))
                {
                    JToken rawToken = tenant.Store.GetRaw(entity);
                    JToken patchToken = JToken.Parse(jsonDiff);
                    var mJsonDiff = new JsonDiffPatchDotNet.JsonDiffPatch();
                    JToken result = mJsonDiff.Patch(rawToken, patchToken);
                    tenant.Store.Watcher.StopMonitoring();
                    tenant.Store.Set(result, entity);
                    var res = tenant.Load(false);
                    if (res.Changes.Length > 0)
                    {
                        _logger.LogTrace("Applied Update: ");
                        foreach (var c in res.Changes)
                            _logger.LogTrace(c.diffToken?.ToString(Newtonsoft.Json.Formatting.Indented));
                    }
                }
                else
                    throw new Exception("Unauthorized Entity: " + entity);
            }
            return true;
        }
    }
}

