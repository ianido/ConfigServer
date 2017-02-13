namespace yupisoft.ConfigServer
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;

    public partial class Startup
    {
        private static void ConfigureAPISecurity(IApplicationBuilder app)
        {
            app.UseCors(options => options.AllowAnyHeader().AllowAnyOrigin());
        }

        public void ConfigureAPISecurityServices(IServiceCollection services)
        {
            services.AddCors();
        }

    }
}
