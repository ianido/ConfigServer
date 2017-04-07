using System;
using System.Collections.Generic;
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

        public ScriptServiceCheck(JServiceCheckConfig checkConfig, JServiceConfig serviceConfig) : base(checkConfig, serviceConfig)
        {
            _checkConfig = checkConfig;
            _checkConfig.Script = _checkConfig.Script.Replace("$address", serviceConfig.Address);
            _checkConfig.Script = _checkConfig.Script.Replace("$port", serviceConfig.Port.ToString());

        }

        protected override void CheckAsync(int callid)
        {
            _lastCheckStatus = ServiceCheckStatus.Passing;
            OnCheckDone(Id, _lastCheckStatus, callid);
        }
    }
}
