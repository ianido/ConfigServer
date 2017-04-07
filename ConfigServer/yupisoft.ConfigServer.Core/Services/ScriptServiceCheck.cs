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

        public ScriptServiceCheck(JServiceCheckConfig checkConfig) : base(checkConfig)
        {
            _checkConfig = checkConfig;
        }
        
        public override void Check(int callid)
        {
            _lastCheckStatus = ServiceCheckStatus.Passing;
            OnCheckDone(Id, _lastCheckStatus, callid);
        }
    }
}
