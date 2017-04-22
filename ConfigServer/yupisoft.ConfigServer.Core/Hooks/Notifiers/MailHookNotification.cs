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
        public string EmailBody { get { return _notificationConfig.EmailBody; } }

        public MailHookNotification(JHookNotificationConfig notificationConfig, ILogger logger) : base(notificationConfig, logger)
        {
        }

        public override async void Notify(IHookCheckResult checkResults)
        {
            var objData = JsonConvert.SerializeObject(checkResults, Formatting.None);
            _logger.LogTrace("Hook(" + checkResults.Hook.Id + ") with Notification(" + Id + ") Invoked.");

            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(EmailFrom));
            message.To.Add(MailboxAddress.Parse(Email));
            message.Subject = EmailSubject ?? "Hook(" + checkResults.Hook.Id + ") with Notification(" + Id + ") Invoked.";

            message.Subject = message.Subject.Replace("$statusfrom", checkResults.Data["statusfrom"].Value<string>());
            message.Subject = message.Subject.Replace("$statusto", checkResults.Data["statusto"].Value<string>());
            message.Subject = message.Subject.Replace("$lastupdate", checkResults.Data["lastupdate"].Value<string>());
            message.Subject = message.Subject.Replace("$prevupdate", checkResults.Data["prevupdate"].Value<string>());
            message.Subject = message.Subject.Replace("$description", checkResults.Data["description"].Value<string>());
            message.Subject = message.Subject.Replace("$servicename", checkResults.Data["servicename"].Value<string>());
            message.Subject = message.Subject.Replace("$serviceid", checkResults.Data["serviceid"].Value<string>());
            message.Subject = message.Subject.Replace("$timespan", checkResults.Data["timespan"].Value<string>());
            message.Subject = message.Subject.Replace("$hookid", checkResults.Hook.Id);
            message.Subject = message.Subject.Replace("$checkerid", Id);

            string bodyText = checkResults.Data.ToString(Formatting.Indented);

            if (!string.IsNullOrEmpty(EmailBody)) {

                bodyText = EmailBody;
                bodyText = bodyText.Replace("$statusfrom", checkResults.Data["statusfrom"].Value<string>());
                bodyText = bodyText.Replace("$statusto", checkResults.Data["statusto"].Value<string>());
                bodyText = bodyText.Replace("$lastupdate", checkResults.Data["lastupdate"].Value<string>());
                bodyText = bodyText.Replace("$prevupdate", checkResults.Data["prevupdate"].Value<string>());
                bodyText = bodyText.Replace("$description", checkResults.Data["description"].Value<string>());
                bodyText = bodyText.Replace("$servicename", checkResults.Data["servicename"].Value<string>());
                bodyText = bodyText.Replace("$serviceid", checkResults.Data["serviceid"].Value<string>());
                bodyText = bodyText.Replace("$timespan", checkResults.Data["timespan"].Value<string>());
                bodyText = bodyText.Replace("$hookid", checkResults.Hook.Id);
                bodyText = bodyText.Replace("$checkerid", Id);
            }

            message.Body = new TextPart("plain")
            {
                Text = bodyText
            };

            using (var client = new SmtpClient())
            {
                string smtp = SMTP;
                if (smtp.IndexOf(':') < 0) smtp += ":25";
                string[] parts = SMTP.Split(':');
                client.Connect(parts[0], int.Parse(parts[1]), EmailUseSSL);
                //client.AuthenticationMechanisms.Clear();
                if (!string.IsNullOrEmpty(Username))
                    client.Authenticate(Username, Password);
                await client.SendAsync(message).ContinueWith( (a) => {
                    HookNotificationResponse res = new HookNotificationResponse();
                    res.NotificationId = Id;
                    if ((a.Status == System.Threading.Tasks.TaskStatus.RanToCompletion) && (a.IsCompleted))
                    {
                        res.Data = "Mail sent";
                        res.Result = HookNotificationResult.Success;
                        _logger.LogTrace("Hook(" + checkResults.Hook.Id + ") with Notification(" + Id + ") ");
                    }
                    else
                    {
                        res.Result = HookNotificationResult.Error;
                        _logger.LogTrace("Hook(" + checkResults.Hook.Id + ") with Notification(" + Id + ") Task Failed: " + a.Status);
                    }
                    OnNotificationDone(Id, res);
                    client.Disconnect(true);
                });                
            }
        }
    }
}
