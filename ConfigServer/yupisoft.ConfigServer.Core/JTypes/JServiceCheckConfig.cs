using Newtonsoft.Json.Linq;
using System;
using System.Dynamic;
using yupisoft.ConfigServer.Core.Utils;

namespace yupisoft.ConfigServer.Core
{
    public enum JServiceCheckType
    {
        Tcp,
        Http,
        Script
    }
    public class JServiceCheckConfig
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool Disabled { get; set; }
        public string Http { get; set; }
        public string Tcp { get; set; }
        public string Script { get; set; }
        public bool tlsSkipVerify { get; set; }
        public string Interval { get; set; }
        public string Timeout { get; set; }
        public string Notes { get; set; }
        public string TTL { get; set; }

        public JServiceCheckType CheckType
        {
            get
            {
                if (!string.IsNullOrEmpty(Tcp))
                    return JServiceCheckType.Tcp;
                if (!string.IsNullOrEmpty(Script))
                    return JServiceCheckType.Script;
                return JServiceCheckType.Http;
            }
        }

        public JServiceCheckConfig()
        {
            TTL = "30m";
            Interval = "10s";
            Timeout = "4s";
            Disabled = false;
        }
    }
}
