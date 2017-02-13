using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#if NET462
using System.ServiceProcess;
using Microsoft.AspNetCore.Hosting.WindowsServices;
#endif
namespace yupisoft.ConfigServer
{
    #if NET462
    internal class MyWebHostService : WebHostService
    {
        public MyWebHostService(IWebHost host) : base(host)
        {
        }

        protected override void OnStarting(string[] args)
        {
            base.OnStarting(args);
        }

        protected override void OnStarted()
        {
            base.OnStarted();
        }

        protected override void OnStopping()
        {
            base.OnStopping();
        }
    }

    public static class MyWebHostServiceServiceExtensions
    {
        public static void RunAsMyService(this IWebHost host)
        {
            var webHostService = new MyWebHostService(host);
            ServiceBase.Run(webHostService);
        }
    }
    #endif
}
