using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using yupisoft.ConfigServer.Core.Stores;
using yupisoft.ConfigServer.Core.Watchers;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace yupisoft.ConfigServer.Core
{
    public class ConfigServerTenant
    {
        public TenantConfigSection TenantConfig { get; set; }
        public IStoreProvider Store { get; set; }
        public JToken Token { get; set; }
        public ConfigServerTenant(TenantConfigSection tenantConfig, IServiceProvider serviceProvider)
        {
            TenantConfig = tenantConfig;
            switch (tenantConfig.Store.Provider)
            {
                case "FileStoreProvider":
                    {
                        Store = new FileStoreProvider(tenantConfig.Store.Connection, tenantConfig.Store.StartEntityName,
                                                      new ConfigWatcher<FileWatcherProvider>(serviceProvider.GetService<ILogger<IConfigWatcher>>()));
                    }
                    break;
                case "SqlServerStoreProvider":
                    {
                        Store = new SqlServerStoreProvider(tenantConfig.Store.Connection, tenantConfig.Store.StartEntityName,
                                                      new ConfigWatcher<SqlServerWatcherProvider>(serviceProvider.GetService<ILogger<IConfigWatcher>>()));
                    }
                    break;
                case "MongoStoreProvider":
                    {
                        Store = new MongoStoreProvider(tenantConfig.Store.Connection, tenantConfig.Store.StartEntityName,
                                                      new ConfigWatcher<MongoWatcherProvider>(serviceProvider.GetService<ILogger<IConfigWatcher>>()));
                    }
                    break;
            }
        }
    }
}
