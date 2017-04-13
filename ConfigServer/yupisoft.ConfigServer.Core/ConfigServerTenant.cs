using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using yupisoft.ConfigServer.Core.Stores;
using yupisoft.ConfigServer.Core.Watchers;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using yupisoft.ConfigServer.Core.Cluster;
using yupisoft.ConfigServer.Core.Utils;
using yupisoft.ConfigServer.Core.Services;

namespace yupisoft.ConfigServer.Core
{
    public class LoadDataResult
    {
        public string DataHash { get; set; }
        public EntityChanges[] Changes { get; set; }
    }
    public class EntityChanges
    {
        public string entity { get; set; }
        public JToken diffToken { get; set; }
    }
    public class ConfigServerTenant
    {
        public TenantConfigSection TenantConfig { get; private set; }
        public IStoreProvider Store { get; }
        public string Id { get { return TenantConfig.Id; } }
        public string Name { get { return TenantConfig.Name; } }
        public bool EnableServiceDiscovery { get { return TenantConfig.EnableServiceDiscovery; } }
        public bool Encrypted { get { return TenantConfig.Encrypted; } }
        public string StartEntityName { get { return Store.StartEntityName; } }
        public string ACLEntityName { get { return Store.ACLEntityName; } }
        public JToken Token { get; private set; }
        public JTenantACL ACL {
            get {
                if (ACLToken != null) return ACLToken.ToObject<JTenantACL>();
                return null;
            }
        }
        public JToken ACLToken { get; private set; }
        public Dictionary<string, JToken> RawTokens { get; private set; }
        public Dictionary<string, Service> Services { get; private set; }

        public string DataHash {
            get
            {
                string allData = "";
                foreach(var v in RawTokens)
                    allData += v.Value.ToString(Newtonsoft.Json.Formatting.None);                    
                ulong dataHash = StringHandling.CalculateHash(allData.ToString());
                return dataHash.ToString();
            }
        }
        private ILogger _logger { get; set; }
        private ConfigServerServices _serviceManager { get; set; }

        public ConfigServerTenant(TenantConfigSection tenantConfig, IHostingEnvironment env, ILogger logger)
        {
            _logger = logger;
            RawTokens = new Dictionary<string, JToken>();
            Services = new Dictionary<string, Service>();
            TenantConfig = tenantConfig;

            switch (tenantConfig.Store.Provider)
            {
                 
                case "FileStoreProvider":
                    {
                        if (tenantConfig.Store.Connection.Contains("$ContentRoot") && env != null)
                            tenantConfig.Store.Connection = tenantConfig.Store.Connection.Replace("$ContentRoot", env.ContentRootPath);
                        Store = new FileStoreProvider(tenantConfig.Store, new ConfigWatcher<FileWatcherProvider>(logger), logger);
                    }
                    break;
                case "SqlServerStoreProvider":
                    {
                        Store = new SqlServerStoreProvider(tenantConfig.Store, new ConfigWatcher<SqlServerWatcherProvider>(logger), logger);
                    }
                    break;
                case "MongoStoreProvider":
                    {
                        Store = new MongoStoreProvider(tenantConfig.Store, new ConfigWatcher<MongoWatcherProvider>(logger), logger);
                    }
                    break;
            }
        }

        private void DiscoverServices(JToken token)
        {
            lock (token)
            {
                JToken[] services = token.SelectTokens("$..$service").ToArray();
                Services.Clear();
                foreach (var s in services)
                {
                    JServiceConfig service = s.Parent.Parent.ToObject<JServiceConfig>();
                    Services.Add(service.Id, new Service(service));
                }
            }
        }

        public LoadDataResult Load(bool startingUp, string entityName)
        {
            List<EntityChanges> tchanges = new List<EntityChanges>();
            Store.Watcher.StopMonitoring();
            Store.Watcher.ClearWatcher();
            Token = Store.Get(StartEntityName);
            if (!string.IsNullOrEmpty(ACLEntityName)) ACLToken = Store.GetRaw(ACLEntityName);

            DiscoverServices(Token);

            var newRawTokens = new Dictionary<string, JToken>();
            if (startingUp)
            {
                newRawTokens.Add(StartEntityName, Store.GetRaw(StartEntityName));
                if (!string.IsNullOrEmpty(ACLEntityName)) newRawTokens.Add(ACLEntityName, ACLToken);
            }
            else
            {
                foreach (var entity in Store.Watcher.GetEntities())
                {
                    var rawToken = Store.GetRaw(entity);
                    if (!startingUp && RawTokens.ContainsKey(entity))
                    {
                        var previousToken = RawTokens[entity];
                        var jsonDiff = new JsonDiffPatchDotNet.JsonDiffPatch();
                        JToken diffToken = jsonDiff.Diff(previousToken, rawToken);
                        if (diffToken != null) tchanges.Add(new EntityChanges() { entity = entity, diffToken = diffToken });
                    }
                    newRawTokens.Add(entity, rawToken);
                }
            }
            RawTokens.Clear();
            RawTokens = newRawTokens;
            Store.Watcher.StartMonitoring();
            return new LoadDataResult() { Changes = tchanges.ToArray(), DataHash = this.DataHash };
        }
    }

    public class ConfigServerTenants
    {
        public List<ConfigServerTenant> Tenants { get; set; }

        public ConfigServerTenants(IOptions<TenantsConfigSection> tenantsConfig, IHostingEnvironment env, ILogger<ConfigServerTenant> logger)
        {
            Tenants = tenantsConfig.Value.Tenants.Where(t=>t.Enabled).Select(t =>
            {
                ConfigServerTenant tenant = new ConfigServerTenant(t, env, logger);
                return tenant;
            }).ToList();
        }


    }
}
