using System.Linq;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Diagnostics;
using System;
#if NET462
using Microsoft.AspNetCore.Hosting.WindowsServices;
#endif
namespace yupisoft.ConfigServer
{
    public class Program
    {
        public static string NodeUrl = "http://localhost:8002";
        public static string NodeName = "8002";
        static void Main(string[] args)
        {            
            if ((args != null) && (args.Length > 0))
            {
                var url = args.FirstOrDefault(p => p.StartsWith("-url"));
                if (url != null)
                {
                    string[] v = url.Split(':');
                    if (v.Length > 3) NodeUrl = v[1] + ":" + v[2] + ":" + v[3];
                    else if (v.Length > 2) NodeUrl = v[1] + ":" + v[2];
                    else if (v.Length > 1) NodeUrl = v[1];                    
                }                
                var sNodeName = args.FirstOrDefault(p => p.StartsWith("-node"));
                if (sNodeName != null)
                {
                    string[] v = sNodeName.Split(':');
                    if (v.Length > 1) NodeName = v[1];
                } else
                {
                    Uri uri = new Uri(NodeUrl);
                    NodeName = uri.Port.ToString();
                }
            }

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls(NodeUrl)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            Core.ConfigServerManager confManager = (Core.ConfigServerManager)Microsoft.Extensions.DependencyInjection.ActivatorUtilities.GetServiceOrCreateInstance(host.Services, typeof(Core.ConfigServerManager));
            confManager.StartServer();



            if (Debugger.IsAttached || args.Contains("-debug"))
            {
                host.Run();
            }
            else
            {
#if NET462
                host.RunAsMyService();
#endif
            }
        }

    }
}
