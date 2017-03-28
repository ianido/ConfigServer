using System.Linq;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Diagnostics;
#if NET462
using Microsoft.AspNetCore.Hosting.WindowsServices;
#endif
namespace yupisoft.ConfigServer
{
    public class Program
    {
        public static int Port = 8002;
        public static string NodeName = Port.ToString();
        static void Main(string[] args)
        {            
            if ((args != null) && (args.Length > 0))
            {
                var sport = args.FirstOrDefault(p => p.StartsWith("port"));
                if (sport != null)
                {
                    string[] v = sport.Split(':');
                    if (v.Length > 1) Port = int.Parse(v[1]);
                }
                var sNodeName = args.FirstOrDefault(p => p.StartsWith("node"));
                if (sNodeName != null)
                {
                    string[] v = sNodeName.Split(':');
                    if (v.Length > 1) NodeName = v[1];
                }
            }

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls("http://localhost:" + Port)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            Microsoft.Extensions.DependencyInjection.ActivatorUtilities.GetServiceOrCreateInstance(host.Services, typeof(Controllers.ConfigController));

            if (Debugger.IsAttached || args.Contains("--debug"))
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
