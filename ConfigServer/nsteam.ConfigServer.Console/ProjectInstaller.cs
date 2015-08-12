using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;

namespace nsteam.ConfigServer.Console
{
    [RunInstaller(true)]
    public class ProjectInstaller : System.Configuration.Install.Installer
    {
        private System.ServiceProcess.ServiceProcessInstaller serviceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller serviceInstaller;


        public ProjectInstaller()
        {
            string _exePath = Assembly.GetExecutingAssembly().Location;
            this.serviceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.serviceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // serviceProcessInstaller1
            // 
            this.serviceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.NetworkService;
            this.serviceProcessInstaller.Password = null;
            this.serviceProcessInstaller.Username = null;
            // 
            // serviceInstaller1
            // 
            this.serviceInstaller.Description = "Config-Server";
            this.serviceInstaller.DisplayName = "Config-Server";
            this.serviceInstaller.ServiceName = "Config-Server";
            this.serviceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
                this.serviceProcessInstaller,
                this.serviceInstaller
            });
        }

        public static void Install(bool undo, string[] args)
        {
            try
            {
                System.Console.WriteLine(undo ? "uninstalling" : "installing");
                using (AssemblyInstaller inst = new AssemblyInstaller(typeof(Program).Assembly, args))
                {
                    IDictionary state = new Hashtable();
                    inst.UseNewContext = true;
                    try
                    {
                        if (undo)
                        {
                            inst.Uninstall(state);
                        }
                        else
                        {
                            inst.Install(state);
                            inst.Commit(state);
                        }
                    }
                    catch
                    {
                        try
                        {
                            inst.Rollback(state);
                        }
                        catch { }
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.Error.WriteLine(ex.Message);
            }
        }

    }
}
