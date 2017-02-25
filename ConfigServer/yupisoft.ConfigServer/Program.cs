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
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls("http://localhost:8000")
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
