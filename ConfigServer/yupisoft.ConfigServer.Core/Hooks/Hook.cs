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
        protected object _sync = new object();
        protected ILogger _logger;
        protected HookCheckStatus _lastCheckStatus;
        protected DateTime _lastChecked = DateTime.UtcNow;
        protected List<IHookNotification> Notifications;
        protected JHookConfig Config { get; private set; }

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

        public event CheckDoneEventHandler CheckDone;
        public event CheckStartedEventHandler CheckStarted;

        protected virtual void OnCheckDone(HookCheckStatus status)
        {
            CheckDone?.Invoke(status);
        }
        protected virtual void OnCheckStarted()
        {
            CheckStarted?.Invoke();
        }

        protected Hook(JHookConfig config, ILogger logger)
        {
            _logger = logger;
            Config = config;
            Notifications = new List<IHookNotification>();
            foreach (var notification in config.Notifications)
            {
                if (notification.Disabled) continue;
                var not = HookNotification.CreateFromConfig(notification, logger);
                if (not != null)
                    Notifications.Add(not);
            }
        }

        public static Hook CreateHook(JHookConfig config, ILogger logger, ConfigServerTenant tenant)
        {            
            if (config.HookType == JHookCheckType.DataNodeChange) return new HookDataNodeChange(config, logger, tenant);
            if (config.HookType == JHookCheckType.ServiceStatusChange) return new HookServiceStatusChange(config, logger, tenant);
            logger.LogWarning("Cant Create a Hook type:" + config.HookType.ToString());
            return null;
        }

        protected abstract Task<IHookCheckResult> CheckAsync();

        public async void Check()
        {
            if ((DateTime.UtcNow - _lastChecked) > Interval) { OnCheckStarted();  _lastChecked = DateTime.UtcNow; _lastCheckStatus = (await CheckAsync()).Result; }
        }

        public void UpdateFrom(JHookConfig config)
        {
            Config = config;
        }
    }
}
