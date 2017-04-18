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
        public ClusterManager ClusterMan { get; private set; }

        public DateTime AliveSince { get; private set; }

        public event DataChangedEventHandler DataChanged;

        public ConfigServerManager(ConfigServerServices serviceManager, ConfigServerHooks hooksManager, ClusterManager clusterMan, ILogger<ConfigServerManager> logger)
        {
            TenantManager = clusterMan.TenantManager;
            ServiceManager = serviceManager;
            HooksManager = hooksManager;
            ClusterMan = clusterMan;
            _logger = logger;   
        }

        public void StartServer()
        {
            foreach (var tenant in TenantManager.Tenants)
            {
                tenant.Store.Change += Store_Change;
                tenant.StartLoadTenantData += Tenant_StartLoadTenantData;
                tenant.EndLoadTenantData += Tenant_EndLoadTenantData;
                tenant.Load(true);
                _logger.LogTrace("Loaded Data for Tenant: " + tenant.Id);
            }
            ClusterMan.StartManaging();
        }

        private void Tenant_EndLoadTenantData(ConfigServerTenant tenant, JToken dataToken, bool startingUp)
        {   
            ServiceManager.StartMonitoring();
            HooksManager.StartMonitoring();
            ServiceManager.StartServiceDiscovery();
        }

        private void Tenant_StartLoadTenantData(ConfigServerTenant tenant, JToken dataToken, bool startingUp)
        {
            ServiceManager.StopServiceDiscovery();
            ServiceManager.StopMonitoring();
            HooksManager.StopMonitoring();            
        }

        private void Store_Change(ConfigServerTenant tenant, IStoreProvider sender, string entityName)
        {
            var loadResult = tenant.Load(false);
            if (loadResult.Changes.Length > 0)
                foreach (var e in loadResult.Changes)
                    DataChanged?.Invoke(tenant.TenantConfig.Id, e.entity, e.diffToken);
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
            return tenant.GetRaw(path, entityName);
        }

        public JToken Get(string path, string tenantId)
        {
            return Get<JToken>(path, tenantId);
        }

        public T Get<T>(string path, string tenantId)
        {
            var tenant = GetTenant(tenantId);
            if (tenant == null)
            {
                _logger.LogError("Get: Tenant: " + tenantId + " not found.");
                new Exception("Get: Tenant: " + tenantId + " not found.");
            }
            return tenant.Get<T>(path);
        }

        public bool Set(JNode newToken, string tenantId)
        {
            var tenant = GetTenant(tenantId);

            if (tenant == null)
            {
                _logger.LogError("Set: Tenant: " + tenantId + " not found.");
                new Exception("Set: Tenant: " + tenantId + " not found.");
            }
            return tenant.Set(newToken, tenantId, false);
        }
        
    }
}

