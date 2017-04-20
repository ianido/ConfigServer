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
        Email
    }
    public class JHookNotificationConfig
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool Disabled { get; set; }
        public string Http { get; set; }
        public string HttpMethod { get; set; }
        public string Email { get; set; }
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
                return JHookNotificationType.Http;
            }
        }

        public JHookNotificationConfig()
        {
            Disabled = false;
            HttpMethod = "post";
        }
    }
}
