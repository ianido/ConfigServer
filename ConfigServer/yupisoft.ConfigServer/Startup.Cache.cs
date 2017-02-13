namespace yupisoft.ConfigServer
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using NLog.Extensions.Logging;
    using NLog.Web;

    public partial class Startup
    {
        private static void ConfigureCachingServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDistributedRedisCache(options =>
                {
                    options.Configuration = configuration["Cache:Redis:Server"];
                    options.Configuration = configuration["Cache:Redis:InstanceName"];
                });
        }
    }
}