using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace yupisoft.ConfigServer
{
    public partial class Startup
    {
        private readonly IConfiguration configuration;
        private readonly IHostingEnvironment hostingEnvironment;

        public Startup(IHostingEnvironment hostingEnvironment)
        {
            this.configuration = ConfigureConfiguration(hostingEnvironment);
            this.hostingEnvironment = hostingEnvironment;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory, IServiceProvider provider)
        {
            ConfigureLogging(app, loggerFactory, this.configuration);
            ConfigureAPISecurity(app);          
            
            app.UseMvc();
        }


        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {

            // First add services that are intrinsic for ServiceCollection
            services.AddAuthentication();
            ConfigureLoggingServices(services);
            ConfigureAPISecurityServices(services);
            //ConfigureCachingServices(services, configuration);

            IMvcBuilder mvcBuilder = services.AddMvc(
                mvcOptions =>
                {
                    ConfigureFilters(this.hostingEnvironment, mvcOptions.Filters);
                }).AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
                }); 
            
            services.AddSingleton<IConfiguration>(imp => configuration);
        }

    }
}

