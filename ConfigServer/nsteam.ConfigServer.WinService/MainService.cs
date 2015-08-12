using NLog.Internal;
using nsteam.ConfigServer.Types;
using SimpleInjector;
using SimpleInjector.Integration.WebApi;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Web.Http;

namespace nsteam.ConfigServer.WinService
{
    public partial class MainService : ServiceBase
    {

        public MainService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Startup.StartServer(new ConfigurationManager().AppSettings["serveraddr"]);
        }

        protected override void OnStop()
        {
            Startup.CloseServer();
        }


        private static Container ConfigureContainer(HttpConfiguration config)
        {
            Container container = new Container();

            #region Register Dependencies

            container.RegisterSingle<ILoggerService, NLogLoggerService>();
            container.RegisterSingle<IConfigurationService, AppConfigService>();

            #endregion

            container.RegisterWebApiControllers(config);
            container.Verify();
            config.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);

            return container;
        }
    }
}
