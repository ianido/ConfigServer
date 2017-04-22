using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace yupisoft.ConfigServer.Core.Hooks
{
    public class ScriptHookNotification : HookNotification
    {
        public string Script { get { return _notificationConfig.Script; } }

        public ScriptHookNotification(JHookNotificationConfig notificationConfig, ILogger logger) : base(notificationConfig, logger)
        {
        }

        public override async void Notify(IHookCheckResult checkResults)
        {
            string script = Script;
            script = script.Replace("$appdir", Directory.GetCurrentDirectory());
            script = script.Replace("$basedir", Directory.GetCurrentDirectory());
            _logger.LogTrace("Hook(" + checkResults.Hook.Id + ") with Notification(" + Id + ") Invoked.");
            await Task.Factory.StartNew(() => {
                Process proc = Process.Start(script);
                proc.WaitForExit(5000);
                HookNotificationResponse res = new HookNotificationResponse();
                res.NotificationId = Id;
                if (proc.ExitCode == 0)
                {
                    res.Data = proc.StandardOutput?.ReadToEnd();
                    res.Result = HookNotificationResult.Success;
                    _logger.LogTrace("Hook(" + checkResults.Hook.Id + ") with Notification(" + Id + ") StatusCode: 0");
                }
                else
                {
                    _logger.LogTrace("Hook(" + checkResults.Hook.Id + ") with Notification(" + Id + ") Task Failed: ");
                    res.Result = HookNotificationResult.Error;
                }
                OnNotificationDone(Id, res);
            }).WithTimeout(TimeSpan.FromSeconds(5));            
        }
    }
}
