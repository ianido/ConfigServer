using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using yupisoft.ConfigServer.Core.Cluster;

namespace yupisoft.ConfigServer.Core
{
    public static class ConfigServerExtensions
    {
        public static IServiceCollection AddConfigServer(this IServiceCollection services, IConfiguration configuration)
        {
            var section =
                configuration.GetSection("ConfigServer");
            // we first need to create an instance
            var tenantSettings = new TenantsConfigSection();
            var clusterSettings = new ClusterConfigSection();

            services.Configure<TenantsConfigSection>(configuration.GetSection("ConfigServer"));
            services.Configure<ClusterConfigSection>(configuration.GetSection("ConfigServer"));

            // then we set the properties 
            //new ConfigureFromConfigurationOptions<TenantsConfigSection>(section).Configure(tenantSettings);
            //new ConfigureFromConfigurationOptions<ClusterConfigSection>(section).Configure(clusterSettings);

            services.AddSingleton<ConfigServerTenants>();// (imp => new ConfigServerTenants(tenantSettings, imp));
            services.AddSingleton<ConfigServerManager>();// (imp => new ConfigServerManager(imp.GetService<ConfigServerTenants>()));
            services.AddSingleton<ClusterManager>(); // (imp => new ClusterManager(imp.GetService<ConfigServerTenants>()));

            return services;
        }

    }
}
