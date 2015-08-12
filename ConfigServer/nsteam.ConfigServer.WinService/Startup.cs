using Microsoft.Owin.Hosting;
using Owin;
using SimpleInjector;
using SimpleInjector.Integration.WebApi;
using System;
using System.Net.Http;
using System.Web.Http;

namespace nsteam.ConfigServer.Types
{
    public class Startup
    {
        private static readonly Container _container = ConfigureContainer();
        private ILoggerService _logger = null;
        private IDisposable _webapp = null;

        public ILoggerService Logger { get { return _logger; } }
        public IDisposable Webapp { get { return _webapp; } set { _webapp = value; } }

        public static void StartServer(string baseAddress)
        {
            var startup = _container.GetInstance<Startup>();
            startup.Logger.Log("ConfigService open", EventType.Info);
            startup.Webapp = WebApp.Start(baseAddress, startup.Configuration);
            startup.Logger.Log(string.Format(@"ConfigServicerHost-WebAPI is running in : {0}", baseAddress), EventType.Info);

            startup.Logger.Log(string.Format("Loading Configuration..."), EventType.Info);
            HttpClient client = new HttpClient();
            var response = client.GetAsync(baseAddress + "api/node/").Result;
            startup.Logger.Log(string.Format("Configuration loaded."), EventType.Info);
        }

        public static void CloseServer()
        {
            var startup = _container.GetInstance<Startup>();
            startup.Webapp.Dispose();
            startup.Logger.Log("ConfigService closed", EventType.Info);
        }


        public Startup(ILoggerService logger)
        {
            _logger = logger;
        }

        public void Configuration(IAppBuilder appBuilder)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Formatters.Remove(config.Formatters.XmlFormatter);
            config.Formatters.Add(new CustomXmlFormatter());

            ConfigureOAuth(appBuilder);

            appBuilder.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            appBuilder.UseWebApi(config);

            _container.Verify();
            config.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(_container);

        }

        private static Container ConfigureContainer()
        {
            Container container = new Container();

            #region Register Dependencies

            container.RegisterSingle<Startup>();
            container.RegisterSingle<ILoggerService, NLogLoggerService>();
            container.RegisterSingle<IConfigurationService, AppConfigService>();

            #endregion

            return container;
        }

        public void ConfigureOAuth(IAppBuilder app)
        {


        }
    }

}
