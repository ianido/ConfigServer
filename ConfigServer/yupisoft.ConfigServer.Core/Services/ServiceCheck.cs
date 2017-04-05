using System;
using System.Collections.Generic;
using System.Text;

namespace yupisoft.ConfigServer.Core.Services
{
    public abstract class ServiceCheck : IServiceCheck
    {
        protected JServiceCheckConfig _srvCheck;
        protected ServiceCheckStatus _lastCheckStatus;
        public ServiceCheck(JServiceCheckConfig checkConfig)
        {
            _srvCheck = checkConfig;
            _lastCheckStatus = ServiceCheckStatus.Nocheck;
        }
        public ServiceCheckStatus LastCheckStatus { get { return _lastCheckStatus; } }
        public abstract void Check();
    }
}
