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
        static void Main(string[] args)
        {
            int port = 8000;
            if ((args != null) && (args.Length > 0))
            {
                var sport = args.FirstOrDefault(p => p.StartsWith("port"));
                if (sport != null)
                {
                    string[] v = sport.Split(':');
                    if (v.Length > 1) port = int.Parse(v[1]);
                }
            }

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls("http://localhost:" + port)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

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
