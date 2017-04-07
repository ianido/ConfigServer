using System;
using System.Collections.Generic;
using System.Text;
using yupisoft.ConfigServer.Core.Utils;

namespace yupisoft.ConfigServer.Core.Services
{

    public delegate void CheckDoneEventHandler(string checkerId, ServiceCheckStatus status, int callid);
    public abstract class ServiceCheck : IServiceCheck
    {
        protected JServiceCheckConfig _checkConfig;
        protected ServiceCheckStatus _lastCheckStatus;

        protected virtual void OnCheckDone(string checkerId, ServiceCheckStatus status, int callid)
        {
            CheckDone?.Invoke(checkerId, status, callid);
        }
        public event CheckDoneEventHandler CheckDone;

        public string Id { get { return _checkConfig.Id; } }
        public string Name { get { return _checkConfig.Name; } }
        public string Notes { get { return _checkConfig.Notes; } }
        public TimeSpan Interval
        {
            get
            {
                return ConverterExtensions.ParseHuman(_checkConfig.Interval);
            }
        }
        public TimeSpan Timeout
        {
            get
            {
                return ConverterExtensions.ParseHuman(_checkConfig.Timeout);
            }
        }

        protected ServiceCheck(JServiceCheckConfig checkConfig)
        {
            _checkConfig = checkConfig;
            _lastCheckStatus = ServiceCheckStatus.Nocheck;
        }
        public static ServiceCheck CreateFromConfig(JServiceCheckConfig checkConfig)
        {
            if (checkConfig.CheckType == JServiceCheckType.Http) return new HttpServiceCheck(checkConfig);
            if (checkConfig.CheckType == JServiceCheckType.Script) return new ScriptServiceCheck(checkConfig);
            if (checkConfig.CheckType == JServiceCheckType.Tcp) return new TcpServiceCheck(checkConfig);
            throw new Exception("Cant create a check type.");
        }
        public ServiceCheckStatus LastCheckStatus { get { return _lastCheckStatus; } }
        public abstract void Check(int callid);
    }
}
