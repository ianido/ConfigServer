using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace yupisoft.ConfigServer.Core.Hooks
{
    public delegate void NotificationDoneEventHandler(string NotificationId, HookNotificationResponse response);
    public abstract class HookNotification : IHookNotification
    {
        protected JHookNotificationConfig _notificationConfig;
        protected ILogger _logger;
        public NotificationDoneEventHandler NotificationDone;

        protected virtual void OnNotificationDone(string NotificationId, HookNotificationResponse status)
        {
            NotificationDone?.Invoke(Id, status);
        }
        public string Id { get { return _notificationConfig.Id; } }
        public string Name { get { return _notificationConfig.Name; } }
        public string Notes { get { return _notificationConfig.Notes; } }
        public bool Disabled { get { return _notificationConfig.Disabled; } }

        public HookNotification(JHookNotificationConfig notificationConfig, ILogger logger)
        {
            _notificationConfig = notificationConfig;
            _logger = logger;
        }
        public static HookNotification CreateFromConfig(JHookNotificationConfig notConfig, ILogger logger)
        {
            if (notConfig.CheckType == JHookNotificationType.Http) return new HttpHookNotification(notConfig, logger);
            if (notConfig.CheckType == JHookNotificationType.Script) return new ScriptHookNotification(notConfig, logger);
            if (notConfig.CheckType == JHookNotificationType.Email) return new MailHookNotification(notConfig, logger);
            logger.LogWarning("Cant Create a Hook Notification type:" + notConfig.CheckType.ToString());
            return null;
        }

        public abstract void Notify(IHookCheckResult checkResults);
    }
}
