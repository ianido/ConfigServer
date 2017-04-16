using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Dynamic;

namespace yupisoft.ConfigServer.Core
{
    public enum JHookCheckType
    {
        DataNodeChange
    }
    public class JHookConfig
    {        
        public string Id { get; set; }
        [JsonProperty(PropertyName = "$hook")]
        public string Type { get; set; }
        public string Description { get; set; }
        public string Node { get; set; }
        public string Http { get; set; }
        public string HttpMethod { get; set; }
        public string Interval { get; set; }
        public string Timeout { get; set; }

        public JHookCheckType HookType
        {
            get
            {
                if (Type == "$nodechange")
                    return JHookCheckType.DataNodeChange;

                return JHookCheckType.DataNodeChange;
            }
        }

        public JHookConfig()
        {
            HttpMethod = "post";
            Interval = "10s";
            Timeout = "2s";
        }

    }
}
