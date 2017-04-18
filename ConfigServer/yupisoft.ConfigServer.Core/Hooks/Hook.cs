using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using yupisoft.ConfigServer.Core.Utils;

namespace yupisoft.ConfigServer.Core.Hooks
{

    public delegate void CheckDoneEventHandler(HookCheckStatus status);
    public delegate void CheckStartedEventHandler();

    public abstract class Hook 
    {
        protected object _sync1 = new object();
        protected ILogger _logger;
        protected HookCheckStatus _lastCheckStatus;
        protected DateTime _lastChecked = DateTime.UtcNow;

        public event CheckDoneEventHandler CheckDone;
        public event CheckStartedEventHandler CheckStarted;

        public string Id { get { return Config.Id; } }
        public string Description { get { return Config.Description; } }
        public TimeSpan Interval
        {
            get
            {
                return ConverterExtensions.ParseHuman(Config.Interval);
            }
        }
        public TimeSpan Timeout
        {
            get
            {
                return ConverterExtensions.ParseHuman(Config.Timeout);
            }
        }
        protected JHookConfig Config { get; private set; }
        protected ConfigServerTenant _Tenant { get; private set; }

        protected virtual void OnCheckDone(HookCheckStatus status)
        {
            CheckDone?.Invoke(status);
        }
        protected virtual void OnCheckStarted()
        {
            CheckStarted?.Invoke();
        }

        protected Hook(JHookConfig config, ILogger logger, ConfigServerTenant tenant)
        {
            _Tenant = tenant;
            _logger = logger;
            Config = config;
        }

        public static Hook CreateHook(JHookConfig config, ILogger logger, ConfigServerTenant tenant)
        {            
            if (config.HookType == JHookCheckType.DataNodeChange) return new HookDataNodeChange(config, logger, tenant);
            throw new Exception("Cant create a check type.");
        }

        protected abstract Task<HookCheckStatus> CheckAsync();

        public async void Check()
        {
            if ((DateTime.UtcNow - _lastChecked) > Interval) { OnCheckStarted();  _lastChecked = DateTime.UtcNow; _lastCheckStatus = await CheckAsync(); }
        }

        public void UpdateFrom(JHookConfig config)
        {
            Config = config;
        }
    }
}
