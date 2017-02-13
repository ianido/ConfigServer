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
        private const string LoggingConfigurationSectionName = "Logging";

        /// <summary>
        /// Configure tools used to help with logging internal application events.
        /// See http://docs.asp.net/en/latest/fundamentals/logging.html
        /// </summary>
        /// <param name="environment">The environment the application is running under. This can be Development,
        /// Staging or Production by default.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="configuration">Gets or sets the application configuration, where key value pair settings are
        /// stored.</param>
        private static void ConfigureLogging(
            IApplicationBuilder app,
            ILoggerFactory loggerFactory,
            IConfiguration configuration)
        {
            loggerFactory.AddConsole(configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            loggerFactory.AddNLog();
            app.AddNLogWeb();

            // Log warning level messages and above to the Windows event log.
            // var sourceSwitch = new SourceSwitch("EventLog");
            // sourceSwitch.Level = SourceLevels.Information;
            // loggerFactory.AddTraceSource(sourceSwitch, new EventLogTraceListener("Application"));

            // Log to Serilog (See https://github.com/serilog/serilog-framework-logging).
            // loggerFactory.AddSerilog();
        }

        private static void ConfigureLoggingServices(IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddLogging();
        }
    }
}