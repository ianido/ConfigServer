using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace yupisoft.ConfigServer.Core.Services
{
    public class ScriptServiceCheck : ServiceCheck
    {       
        private object _locking = new object();
        private JServiceConfig _serviceConfig;

        public ScriptServiceCheck(JServiceCheckConfig checkConfig, JServiceConfig serviceConfig, ILogger logger) : base(checkConfig, logger)
        {
            _serviceConfig = serviceConfig;
        }

        protected override void CheckAsync()
        {
            var script = _checkConfig.Script;
            script = script.Replace("$appdir", Directory.GetCurrentDirectory());
            script = script.Replace("$basedir", Directory.GetCurrentDirectory());
            script = script.Replace("$address", _serviceConfig.Address);
            script = script.Replace("$port", _serviceConfig.Port.ToString());

            Task.Factory.StartNew(() => {
                Process proc = Process.Start(script);
                proc.WaitForExit(Convert.ToInt32(Timeout.TotalMilliseconds));
                if (proc.ExitCode == 0)
                    _lastCheckStatus = ServiceCheckStatus.Passing;
                else
                    _lastCheckStatus = ServiceCheckStatus.Failing;
                OnCheckDone(Id, _lastCheckStatus);
            }).WithTimeout(Timeout.Add(TimeSpan.FromSeconds(1)));         
        }

    }
}
