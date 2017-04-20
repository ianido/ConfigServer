using Newtonsoft.Json.Linq;
using System;
using System.Dynamic;
using yupisoft.ConfigServer.Core.Utils;

namespace yupisoft.ConfigServer.Core
{
    public enum JHookNotificationType
    {
        Http,
        Script,
        Email,
        Unknow
    }
    public class JHookNotificationConfig
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool Disabled { get; set; }
        public string Http { get; set; }
        public string HttpMethod { get; set; }
        public string Email { get; set; }
        public string EmailFrom { get; set; }
        public string SMTP { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool EmailUseSSL { get; set; }
        public string EmailSubject { get; set; }
        public string Script { get; set; }
        public string Notes { get; set; }

        public JHookNotificationType CheckType
        {
            get
            {
                if (!string.IsNullOrEmpty(Email))
                    return JHookNotificationType.Email;
                if (!string.IsNullOrEmpty(Script))
                    return JHookNotificationType.Script;
                if (!string.IsNullOrEmpty(Http))
                    return JHookNotificationType.Http;
                return JHookNotificationType.Unknow;
            }
        }

        public JHookNotificationConfig()
        {
            Disabled = false;
            HttpMethod = "post";
        }
    }
}
