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

        public ScriptServiceCheck(JServiceCheckConfig checkConfig, JServiceConfig serviceConfig, ILogger logger) : base(checkConfig, serviceConfig, logger)
        {
            _checkConfig = checkConfig; 
            _checkConfig.Script = _checkConfig.Script.Replace("$appdir", Directory.GetCurrentDirectory());
            _checkConfig.Script = _checkConfig.Script.Replace("$basedir", Directory.GetCurrentDirectory());
            _checkConfig.Script = _checkConfig.Script.Replace("$address", serviceConfig.Address);
            _checkConfig.Script = _checkConfig.Script.Replace("$port", serviceConfig.Port.ToString());
        }

        protected override void CheckAsync()
        {
            Task.Factory.StartNew(() => {
                Process proc = Process.Start(_checkConfig.Script);
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
