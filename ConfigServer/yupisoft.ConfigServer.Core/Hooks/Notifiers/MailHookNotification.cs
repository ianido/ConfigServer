using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using MimeKit;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using yupisoft.ConfigServer.Core.Utils;

namespace yupisoft.ConfigServer.Core.Hooks
{
    public class MailHookNotification : HookNotification
    {
        public string Email { get { return _notificationConfig.Email; } }
        public string EmailFrom { get { return _notificationConfig.EmailFrom; } }
        public string SMTP { get { return _notificationConfig.SMTP; } }
        public string Username { get { return _notificationConfig.Username; } }
        public string Password { get { return _notificationConfig.Password; } }
        public bool EmailUseSSL { get { return _notificationConfig.EmailUseSSL; } }
        public string EmailSubject { get { return _notificationConfig.EmailSubject; } }

        public MailHookNotification(JHookNotificationConfig notificationConfig, ILogger logger) : base(notificationConfig, logger)
        {
        }

        public override async void Notify(IHookCheckResult checkResults)
        {
            var objData = JsonConvert.SerializeObject(checkResults, Formatting.None);
            _logger.LogTrace("Hook(" + checkResults.HookId + ") with Notification(" + Id + ") Invoked.");

            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(EmailFrom));
            message.To.Add(MailboxAddress.Parse(Email));
            message.Subject = EmailSubject ?? "Hook(" + checkResults.HookId + ") with Notification(" + Id + ") Invoked.";
            message.Subject = message.Subject.Replace("$hookid", checkResults.HookId);
            message.Subject = message.Subject.Replace("$checkerid", Id);
            message.Body = new TextPart("plain")
            {
                Text = checkResults.Data.ToString(Formatting.Indented)
            };

            using (var client = new SmtpClient())
            {
                string smtp = SMTP;
                if (smtp.IndexOf(':') < 0) smtp += ":25";
                string[] parts = SMTP.Split(':');
                client.Connect(parts[0], int.Parse(parts[1]), EmailUseSSL);
                client.AuthenticationMechanisms.Clear();
                if (!string.IsNullOrEmpty(Username))
                    client.Authenticate(Username, Password);
                await client.SendAsync(message).ContinueWith( (a) => {
                    HookNotificationResponse res = new HookNotificationResponse();
                    res.NotificationId = Id;
                    if ((a.Status == System.Threading.Tasks.TaskStatus.RanToCompletion) && (a.IsCompleted))
                    {
                        res.Data = "Mail sent";
                        res.Result = HookNotificationResult.Success;
                        _logger.LogTrace("Hook(" + checkResults.HookId + ") with Notification(" + Id + ") ");
                    }
                    else
                    {
                        res.Result = HookNotificationResult.Error;
                        _logger.LogTrace("Hook(" + checkResults.HookId + ") with Notification(" + Id + ") Task Failed: " + a.Status);
                    }
                    OnNotificationDone(Id, res);
                    client.Disconnect(true);
                });                
            }
        }
    }
}
