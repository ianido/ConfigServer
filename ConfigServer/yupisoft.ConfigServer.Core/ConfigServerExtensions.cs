using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using yupisoft.ConfigServer.Core.Cluster;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace yupisoft.ConfigServer.Core
{
    public static class ConfigServerExtensions
    {
        public static IServiceCollection AddConfigServer(this IServiceCollection services, IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            var section = configuration.GetSection("ConfigServer");
            
            services.Configure<TenantsConfigSection>(section);
            services.Configure<ClusterConfigSection>(section);

            services.AddSingleton<ConfigServerTenants>();
            services.AddSingleton<ConfigServerManager>();
            services.AddSingleton<ConfigurationChanger>(imp => new ConfigurationChanger(Path.Combine(hostingEnvironment.ContentRootPath, "appsettings.json")));
            services.AddSingleton<ClusterManager>(); 

            return services;
        }

    }
}
