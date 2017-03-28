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
            var section = configuration.GetSection("ConfigServer");
            
            services.Configure<TenantsConfigSection>(section);
            services.Configure<ClusterConfigSection>(section);

            services.AddSingleton<ConfigServerTenants>();
            services.AddSingleton<ConfigServerManager>();
            services.AddSingleton<ClusterManager>(); 

            return services;
        }

    }
}
