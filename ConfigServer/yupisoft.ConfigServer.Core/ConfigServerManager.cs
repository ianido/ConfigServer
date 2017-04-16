using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using yupisoft.ConfigServer.Core.Cluster;

namespace yupisoft.ConfigServer.Core
{
    public delegate void DataChangedEventHandler(string tenantId, string entity, JToken jsonDiff);

    public class ConfigServerManager
    {
        private object _lock = new object();

        private ILogger _logger;

        public ConfigServerTenants TenantManager { get; private set; }

        public ConfigServerServices ServiceManager { get; private set; }
        public ConfigServerHooks HooksManager { get; private set; }

        public DateTime AliveSince { get; private set; }

        public event DataChangedEventHandler DataChanged;

        public ConfigServerManager(ConfigServerTenants tenants, ConfigServerServices serviceManager, ConfigServerHooks hooksManager, ILogger<ConfigServerManager> logger)
        {
            TenantManager = tenants;
            ServiceManager = serviceManager;
            HooksManager = hooksManager;

            _logger = logger;
            AliveSince = DateTime.UtcNow;

            foreach (var tenant in TenantManager.Tenants)
            {
                tenant.Store.Change += Store_Change;
                tenant.StartLoadTenantData += Tenant_StartLoadTenantData;
                tenant.EndLoadTenantData += Tenant_EndLoadTenantData;
                tenant.Load(true);
            }
            _logger.LogTrace("Created ConfigManager with " + TenantManager.Tenants.Count + " tenants.");
            serviceManager.StartServiceDiscovery();
        }

        private void Tenant_EndLoadTenantData(ConfigServerTenant tenant, JToken dataToken, bool startingUp)
        {
            ServiceManager.StopMonitoring();
            HooksManager.StopMonitoring();
        }

        private void Tenant_StartLoadTenantData(ConfigServerTenant tenant, JToken dataToken, bool startingUp)
        {
            ServiceManager.StartMonitoring();
            HooksManager.StartMonitoring();
        }

        private void Store_Change(IStoreProvider sender, string entityName)
        {
            ServiceManager.StopServiceDiscovery();
            ServiceManager.StopMonitoring();
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
            ServiceManager.StartMonitoring();
            ServiceManager.StartServiceDiscovery();
        }

        private ConfigServerTenant GetTenant(string tenantId)
        {
            foreach (var tenant in TenantManager.Tenants)
            {
                if (tenant.TenantConfig.Id == tenantId)
                    return tenant;
            }
            return null;
        }

        public JNode GetRaw(string path, string entityName, string tenantId)
        {
            var tenant = GetTenant(tenantId);
            if (tenant == null) throw new Exception("Tenant: " + tenantId + " not found.");
            if (tenant.Token == null) throw new Exception("Tenant: " + tenantId + " not loaded.");

            lock (_lock)
            {
                if (entityName == "@default") entityName = tenant.Store.StartEntityName;
                JToken selToken = tenant.RawTokens[entityName].SelectToken(path);
                if (selToken == null) return new JNode(path, "{}", entityName);
                var result = selToken.ToObject<JToken>();
                return new JNode(path, result, entityName);
            }
        }

        public JToken Get(string path, string tenantId)
        {
            return Get<JToken>(path, tenantId);
        }

        public T Get<T>(string path, string tenantId)
        {
            var tenant = GetTenant(tenantId);
            if (tenant == null) throw new Exception("Tenant: " + tenantId + " not found.");
            if (tenant.Token == null) throw new Exception("Tenant: " + tenantId + " not loaded.");

            lock (_lock)
            {
                JToken selToken = tenant.Token.SelectToken(path);
                if (selToken == null) return default(T);
                var result = selToken.ToObject<T>();
                return result;
            }
        }

        public bool Set(JNode newToken, string tenantId)
        {
            return Set(newToken, tenantId, false);
        }

        internal bool Set(JNode newToken, string tenantId, bool stopMonitoring)
        {

            var tenant = GetTenant(tenantId);

            if (tenant == null) {
                _logger.LogError("ApplyUpdate: Tenant: " + tenantId + " not found.");
                new Exception("ApplyUpdate: Tenant: " + tenantId + " not found.");
            }
            if (tenant.Token == null) {
                _logger.LogWarning("ApplyUpdate: Tenant: " + tenantId + " not loader.");
                new Exception("ApplyUpdate: Tenant: " + tenantId + " not loaded.");
            }

            lock (_lock)
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
                            foreach (var c in res.Changes)
                                _logger.LogTrace(c.diffToken?.ToString(Newtonsoft.Json.Formatting.Indented));
                        }
                    }
                }
                else
                {
                    _logger.LogError("ApplyUpdate: Unauthorized Entity: " + newToken.Entity);
                    throw new Exception("Unauthorized Entity: " + newToken.Entity);
                }
            }
            return true;
        }

        public bool ApplyUpdate(List<LogMessage> logs, List<LogMessage> applyedlogs)
        {
            int TotalLogsApplied = 0;
            var groupedTenant = logs.GroupBy(u => u.TenantId).ToList();

            foreach (var groupT in groupedTenant)
            {
                string tenantId = groupT.Key;

                if (tenantId == null)
                {
                    _logger.LogTrace("<Null> TenantId found.");
                    continue;
                }
                ConfigServerTenant tenant = GetTenant(tenantId);
                if (tenant == null) { _logger.LogError("ApplyUpdate: Tenant: " + tenantId + " not found."); continue; }
                if (tenant.Token == null) { _logger.LogWarning("ApplyUpdate: Tenant: " + tenantId + " not loader."); continue; }
                lock (_lock)
                {
                    tenant.Store.Watcher.StopMonitoring();
                    var groupedEntities = groupT.GroupBy(u => u.Entity).ToList();
                    foreach (var group in groupedEntities)
                    {
                        string entity = group.Key;
                        if (!tenant.Store.Watcher.IsWatching(entity)) { _logger.LogError("ApplyUpdate: Unrecognized entity: " + entity + " for tanant: " + tenant); continue; }

                        var groupSorted = group.OrderBy(g => g.LogId);

                        foreach (var log in groupSorted)
                        {
                            if (string.IsNullOrEmpty(log.JsonDiff)) { continue; }
                            JToken rawToken = tenant.Store.GetRaw(entity);
                            applyedlogs.Add(log);

                            if (log.Full)
                            {
                                tenant.Store.Set(JToken.Parse(log.JsonDiff), entity);
                            }
                            else
                            {
                                JToken patchToken = JToken.Parse(log.JsonDiff);
                                var mJsonDiff = new JsonDiffPatchDotNet.JsonDiffPatch();
                                JToken result = mJsonDiff.Patch(rawToken, patchToken);
                                tenant.Store.Set(result, entity);
                            }
                        }
                    }
                    
                    var res = tenant.Load(false);
                    TotalLogsApplied += res.Changes.Length;
                    if (res.Changes.Length > 0)
                    {
                        _logger.LogTrace("Applied Update: ");
                        foreach (var c in res.Changes)
                            _logger.LogTrace(c.diffToken?.ToString(Newtonsoft.Json.Formatting.Indented));
                    }

                }
            }
            _logger.LogInformation("Applied " + TotalLogsApplied + " logs successfully.");
            return TotalLogsApplied > 0;
        }
    }
}

