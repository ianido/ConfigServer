using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace yupisoft.ConfigServer.Core.Hooks
{
    public class HttpHookNotification : HookNotification
    {
        public string Http { get { return _notificationConfig.Http; } }
        public string HttpMethod { get { return _notificationConfig.HttpMethod; } }

        public HttpHookNotification(JHookNotificationConfig notificationConfig, ILogger logger) : base(notificationConfig, logger)
        {
        }

        public override async void Notify(IHookCheckResult checkResults)
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage request = null;

            if (HttpMethod.ToLower() == "post") request = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, Http);
            if (HttpMethod.ToLower() == "get") request = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, Http);
            if (HttpMethod.ToLower() == "put") request = new HttpRequestMessage(System.Net.Http.HttpMethod.Put, Http);
            if (HttpMethod.ToLower() == "delete") request = new HttpRequestMessage(System.Net.Http.HttpMethod.Delete, Http);
            var objData = JsonConvert.SerializeObject(checkResults, Formatting.None);
            request.Content = new StringContent(objData, Encoding.UTF8, "application/json");
            _logger.LogTrace("Hook(" + checkResults.Hook.Id + ") with Notification(" + Id + ") Invoked.");

            await client.SendAsync(request).ContinueWith((a) =>
            {
                HookNotificationResponse res = new HookNotificationResponse();
                res.NotificationId = Id;
                if ((a.Status == System.Threading.Tasks.TaskStatus.RanToCompletion) && (a.Result.IsSuccessStatusCode))
                {
                    res.Data = a.Result.Content.ReadAsStringAsync().Result;
                    res.Result = HookNotificationResult.Success;
                    _logger.LogTrace("Hook(" + checkResults.Hook.Id + ") with Notification(" + Id + ") StatusCode: " + a.Result.StatusCode);
                }
                else
                {
                    res.Result = HookNotificationResult.Error;
                    _logger.LogTrace("Hook(" + checkResults.Hook.Id + ") with Notification(" + Id + ") Task Failed: " + a.Status);
                }
                OnNotificationDone(Id, res);
            });
        }
    }
}
