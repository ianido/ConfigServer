using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Dynamic;

namespace yupisoft.ConfigServer.Core
{
    public enum JHookCheckType
    {
        DataNodeChange,
        ServiceStatusChange,
        Unknow
    }
    public class JHookConfig
    {        
        public string Id { get; set; }
        [JsonProperty(PropertyName = "$hook")]
        public string Type { get; set; }
        public string Description { get; set; }
        public string Node { get; set; }
        public string ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string NotifyOn { get; set; }
        public JHookNotificationConfig[] Notifications { get; set; }
        public string Interval { get; set; }
        public string Timeout { get; set; }

        public JHookCheckType HookType
        {
            get
            {
                if (Type == "$datanode.change")
                    return JHookCheckType.DataNodeChange;
                if (Type == "$servicestatus.change")
                    return JHookCheckType.ServiceStatusChange;
                return JHookCheckType.Unknow;
            }
        }

        public JHookConfig()
        {

            Interval = "10s";
            Timeout = "2s";
        }

    }
}
