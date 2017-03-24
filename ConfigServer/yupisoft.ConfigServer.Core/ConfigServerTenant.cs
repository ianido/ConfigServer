﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using yupisoft.ConfigServer.Core.Stores;
using yupisoft.ConfigServer.Core.Watchers;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Hosting;

namespace yupisoft.ConfigServer.Core
{
    public class ConfigServerTenant
    {
        public TenantConfigSection TenantConfig { get; set; }
        public IStoreProvider Store { get; }
        public string StartEntityName { get { return Store.StartEntityName; } }
        public JToken Token { get; set; }
        public Dictionary<string, JToken> RawTokens { get; set; }

        public ConfigServerTenant(TenantConfigSection tenantConfig, IServiceProvider serviceProvider)
        {
            RawTokens = new Dictionary<string, JToken>();
            TenantConfig = tenantConfig;
            switch (tenantConfig.Store.Provider)
            {
                 
                case "FileStoreProvider":
                    {
                        var env = serviceProvider.GetService<IHostingEnvironment>();
                        if (tenantConfig.Store.Connection.Contains("$ContentRoot") && env != null)
                            tenantConfig.Store.Connection = tenantConfig.Store.Connection.Replace("$ContentRoot", env.ContentRootPath);
                        Store = new FileStoreProvider(tenantConfig.Store.Connection, tenantConfig.Store.StartEntityName,
                                                      new ClusterManager<FileWatcherProvider>(serviceProvider.GetService<ILogger<IConfigWatcher>>()),
                                                      serviceProvider.GetService<ILogger<IStoreProvider>>());
                    }
                    break;
                case "SqlServerStoreProvider":
                    {
                        Store = new SqlServerStoreProvider(tenantConfig.Store.Connection, tenantConfig.Store.StartEntityName,
                                                      new ClusterManager<SqlServerWatcherProvider>(serviceProvider.GetService<ILogger<IConfigWatcher>>()),
                                                      serviceProvider.GetService<ILogger<IStoreProvider>>());
                    }
                    break;
                case "MongoStoreProvider":
                    {
                        Store = new MongoStoreProvider(tenantConfig.Store.Connection, tenantConfig.Store.StartEntityName,
                                                      new ClusterManager<MongoWatcherProvider>(serviceProvider.GetService<ILogger<IConfigWatcher>>()),
                                                      serviceProvider.GetService<ILogger<IStoreProvider>>());
                    }
                    break;
            }
        }

        public void Load()
        {
            Store.Watcher.StopMonitoring();
            Store.Watcher.ClearWatcher();
            Token = Store.Get(StartEntityName);
            //RawTokens.Clear();
            var newRawTokens = new Dictionary<string, JToken>();
            foreach (var entity in Store.Watcher.GetEntities())
            {
                var rawToken = Store.GetRaw(entity);
                var previousToken = RawTokens["entity"];
                var jsonDiff = new JsonDiffPatchDotNet.JsonDiffPatch();
                JToken diffToken = jsonDiff.Diff(previousToken, rawToken);
                // Sent Changes Diff to clusters.
                newRawTokens.Add(entity, rawToken);
            }
            RawTokens.Clear();
            RawTokens = newRawTokens;
            Store.Watcher.StartMonitoring();
        }
    }

    public class ConfigServerTenants
    {
        public List<ConfigServerTenant> Tenants { get; set; }

        public ConfigServerTenants(TenantsConfigSection tenantsConfig, IServiceProvider serviceProvider)
        {
            Tenants = tenantsConfig.Tenants.Where(t=>t.Enabled).Select(t => new ConfigServerTenant(t, serviceProvider)).ToList();
        }
    }
}
