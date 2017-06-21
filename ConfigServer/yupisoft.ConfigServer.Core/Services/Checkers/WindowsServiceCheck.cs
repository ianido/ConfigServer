using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace yupisoft.ConfigServer.Core.Services
{
    public class WindowsServiceCheck : ServiceCheck
    {       
        private object _locking = new object();
        private JServiceConfig _serviceConfig;

        public WindowsServiceCheck(JServiceCheckConfig checkConfig, JServiceConfig serviceConfig, ILogger logger) : base(checkConfig, logger)
        {
            _serviceConfig = serviceConfig;
        }

        protected override void CheckAsync()
        {
            Task.Factory.StartNew(() => {
                string status = WindowsServiceCheckStatus(_checkConfig.WindowsServiceName);
                if (status == "RUNNING")
                    _lastCheckStatus = ServiceCheckStatus.Passing;
                else
                    _lastCheckStatus = ServiceCheckStatus.Failing;
                OnCheckDone(Id, _lastCheckStatus);
            }).WithTimeout(Timeout.Add(TimeSpan.FromSeconds(1)));         
        }

        private string WindowsServiceCheckStatus(string serviceName)
        {
            
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(Environment.GetEnvironmentVariable("WINDIR"), @"system32\\sc.exe"),
                    Arguments = "\\\\" + _serviceConfig.Address + " query \""+ serviceName + "\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            proc.WaitForExit(Convert.ToInt32(Timeout.TotalMilliseconds));
            string output = proc.StandardOutput.ReadToEnd();

            string pattern = @"STATE[\s|\t]*:[\s|\t]*(\d\d?)[\s|\t]*([A-Z_]*)";
            Match m = Regex.Match(output, pattern);
            string status = "";
            if (m != null && m.Groups != null && m.Groups.Count == 3)
                status = m.Groups[2].Value;
            return status;
        }
    }
}
