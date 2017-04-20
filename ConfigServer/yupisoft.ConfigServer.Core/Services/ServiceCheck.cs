using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using yupisoft.ConfigServer.Core.Utils;

namespace yupisoft.ConfigServer.Core.Services
{

    public delegate void CheckDoneEventHandler(string checkerId, ServiceCheckStatus status);
    public delegate void CheckStartedEventHandler(string checkerId);
    public abstract class ServiceCheck : IServiceCheck
    {
        protected JServiceCheckConfig _checkConfig;
        protected ServiceCheckStatus _lastCheckStatus;
        protected DateTime _lastChecked = DateTime.UtcNow;
        protected ILogger _logger;

        protected virtual void OnCheckDone(string checkerId, ServiceCheckStatus status)
        {
            CheckDone?.Invoke(checkerId, status);
        }
        protected virtual void OnCheckStarted(string checkerId)
        {
            CheckStarted?.Invoke(checkerId);
        }

        public event CheckDoneEventHandler CheckDone;
        public event CheckStartedEventHandler CheckStarted;

        public string Id { get { return _checkConfig.Id; } }
        public string Name { get { return _checkConfig.Name; } }
        public string Notes { get { return _checkConfig.Notes; } }
        public bool Disabled { get { return _checkConfig.Disabled; } }
        public TimeSpan Interval
        {
            get
            {
                return ConverterExtensions.ParseHuman(_checkConfig.Interval);
            }
        }
        public TimeSpan Ttl
        {
            get
            {
                return ConverterExtensions.ParseHuman(_checkConfig.TTL);
            }
        }
        public TimeSpan Timeout
        {
            get
            {
                return ConverterExtensions.ParseHuman(_checkConfig.Timeout);
            }
        }

        protected ServiceCheck(JServiceCheckConfig checkConfig, ILogger logger)
        {
            _checkConfig = checkConfig;
            _lastCheckStatus = ServiceCheckStatus.Nocheck;
            _lastChecked = DateTime.UtcNow;
            _logger = logger;
        }
        public static ServiceCheck CreateFromConfig(JServiceCheckConfig checkConfig, JServiceConfig serviceConfig, ILogger logger)
        {
            if (checkConfig.CheckType == JServiceCheckType.Http) return new HttpServiceCheck(checkConfig, serviceConfig, logger);
            if (checkConfig.CheckType == JServiceCheckType.Script) return new ScriptServiceCheck(checkConfig, serviceConfig, logger);
            if (checkConfig.CheckType == JServiceCheckType.Tcp) return new TcpServiceCheck(checkConfig, serviceConfig, logger);
            logger.LogWarning("Cant Create a Service Check type:" + checkConfig.CheckType.ToString());
            return null;
        }
        public ServiceCheckStatus LastCheckStatus { get { return _lastCheckStatus; } }
        public void Check()
        {
            try
            {
                if ((DateTime.UtcNow - _lastChecked) > Interval) { OnCheckStarted(Id); CheckAsync(); _lastChecked = DateTime.UtcNow; }
                if ((DateTime.UtcNow - _lastChecked) > Ttl) { _lastCheckStatus = ServiceCheckStatus.Failing; }
            }
            catch(Exception ex)
            {
                _logger.LogError("ServiceCheck: Exception performing the check(" + Id +"). Message:" + ex.ToString());
            }
        }
        protected abstract void CheckAsync();
    }
}
