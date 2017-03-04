using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;

namespace yupisoft.ConfigServer.Core
{
    public static class ConfigServerExtensions
    {
        public static IServiceCollection AddConfigServer(this IServiceCollection services, IConfiguration configuration)
        {
            var section =
                configuration.GetSection("ConfigServer");
            // we first need to create an instance
            var settings = new TenantsConfigSection();
            // then we set the properties 
            new ConfigureFromConfigurationOptions<TenantsConfigSection>(section).Configure(settings);

            services.AddSingleton(imp => new ConfigServerTenants(settings, imp));
            services.AddSingleton(imp => new ConfigServerManager(imp.GetService<ConfigServerTenants>()));

            return services;
        }

    }
}
