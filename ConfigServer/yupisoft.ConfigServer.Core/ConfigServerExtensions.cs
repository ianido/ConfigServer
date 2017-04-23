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
using Microsoft.AspNetCore.Builder;
using yupisoft.ConfigServer.Core.Services;

namespace yupisoft.ConfigServer.Core
{
    public static class ConfigServerExtensions
    {
        public static IServiceCollection AddConfigServer(this IServiceCollection services, IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            var mainSection = configuration.GetSection("ConfigServer");
            var datacenterSection = configuration.GetSection("ConfigServer:Datacenter");
            var clusterSection = configuration.GetSection("ConfigServer:Cluster");
            var securitySection = configuration.GetSection("ConfigServer:Cluster:Security");
            var sdSection = configuration.GetSection("ConfigServer:ServiceDiscovery");

            
            services.Configure<DatacenterConfigSection>(datacenterSection);
            services.Configure<TenantsConfigSection>(mainSection);
            services.Configure<ClusterConfigSection>(clusterSection);
            services.Configure<HmacAuthenticationOptions>(securitySection);
            services.Configure<ServiceDiscoveryConfigSection>(sdSection);


            var settings = new TenantsConfigSection();

            services.AddSingleton<IGeoIPServiceProvider, GeoIPAPIProvider>();
            services.AddSingleton<GeoServices>();

            services.AddSingleton<ConfigServerTenants>();            
            services.AddSingleton<ConfigServerManager>();
            services.AddSingleton<ConfigServerServices>();
            services.AddSingleton<ConfigServerHooks>();
            services.AddSingleton<ConfigurationChanger>(imp => new ConfigurationChanger(Path.Combine(hostingEnvironment.ContentRootPath, "appsettings.json")));
            services.AddSingleton<IClusterManager, MergeClusterManager>(); 

            return services;
        }

    }
}
