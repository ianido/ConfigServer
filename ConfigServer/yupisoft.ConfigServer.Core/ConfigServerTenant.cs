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
        public TenantConfigSection TenantConfig { get; set; }
        public IStoreProvider Store { get; }
        public string StartEntityName { get { return Store.StartEntityName; } }
        public JToken Token { get; set; }
        public Dictionary<string, JToken> RawTokens { get; set; }
        public Dictionary<string, TService> Services { get; set; }

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

        public ConfigServerTenant(TenantConfigSection tenantConfig, IHostingEnvironment env, ILogger logger)
        {
            _logger = logger;
            RawTokens = new Dictionary<string, JToken>();
            TenantConfig = tenantConfig;
            switch (tenantConfig.Store.Provider)
            {
                 
                case "FileStoreProvider":
                    {
                        if (tenantConfig.Store.Connection.Contains("$ContentRoot") && env != null)
                            tenantConfig.Store.Connection = tenantConfig.Store.Connection.Replace("$ContentRoot", env.ContentRootPath);
                        Store = new FileStoreProvider(tenantConfig.Store.Connection, tenantConfig.Store.StartEntityName,
                                                      new ConfigWatcher<FileWatcherProvider>(logger), logger);
                    }
                    break;
                case "SqlServerStoreProvider":
                    {
                        Store = new SqlServerStoreProvider(tenantConfig.Store.Connection, tenantConfig.Store.StartEntityName,
                                                      new ConfigWatcher<SqlServerWatcherProvider>(logger), logger);
                    }
                    break;
                case "MongoStoreProvider":
                    {
                        Store = new MongoStoreProvider(tenantConfig.Store.Connection, tenantConfig.Store.StartEntityName,
                                                      new ConfigWatcher<MongoWatcherProvider>(logger), logger);
                    }
                    break;
            }
        }

        public LoadDataResult Load(bool startingUp)
        {
            List<EntityChanges> tchanges = new List<EntityChanges>();
            Store.Watcher.StopMonitoring();
            Store.Watcher.ClearWatcher();
            Token = Store.Get(StartEntityName);
            //RawTokens.Clear();
            var newRawTokens = new Dictionary<string, JToken>();
            foreach (var entity in Store.Watcher.GetEntities())
            {
                var rawToken = Store.GetRaw(entity);
                if (!startingUp && RawTokens.ContainsKey(entity))
                {
                    var previousToken = RawTokens[entity];
                    var jsonDiff = new JsonDiffPatchDotNet.JsonDiffPatch();
                    JToken diffToken = jsonDiff.Diff(previousToken, rawToken);
                    tchanges.Add(new EntityChanges() { entity = entity, diffToken = diffToken });
                }
                newRawTokens.Add(entity, rawToken);
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
