using NLog.Internal;
using nsteam.ConfigServer.Types;
using SimpleInjector;
using SimpleInjector.Integration.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Web.Http;

namespace nsteam.ConfigServer.Console
{
    class Program
    {
        static MainService s = new MainService();

        static int Main(string[] args)
        {
            bool install = false, uninstall = false, console = false, rethrow = false;
            try
            {
                foreach (string arg in args)
                {
                    switch (arg)
                    {
                        case "-i":
                        case "-install":
                            install = true; break;
                        case "-u":
                        case "-uninstall":
                            uninstall = true; break;
                        case "-c":
                        case "-console":
                            console = true; break;
                        default:
                            System.Console.WriteLine("Argument not expected: " + arg);
                            break;
                    }
                }

                if (uninstall)
                {
                    ProjectInstaller.Install(true, args);
                }
                if (install)
                {
                    ProjectInstaller.Install(false, args);
                }

                if (console)
                {
                    StartUp();
                    System.Console.WriteLine("Press Enter to exit.");
                    System.Console.ReadLine();
                    ShutDown();
                }
                else if (!(install || uninstall))
                {
                    rethrow = true; // so that windows sees error...

                    ServiceBase[] ServicesToRun;
                    ServicesToRun = new ServiceBase[] { new MainService() };
                    ServiceBase.Run(ServicesToRun);
                    rethrow = false;
                }
                return 0;
            }
            catch (Exception ex)
            {
                if (rethrow) throw;
                System.Console.WriteLine(ex.ToString());
                return -1;
            }
        }

        static void StartUp()
        {
            Startup.StartServer(new ConfigurationManager().AppSettings["serveraddr"]);
        }

        static void ShutDown()
        {
            Startup.CloseServer();
        }



    }
}
