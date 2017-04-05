using Newtonsoft.Json.Linq;
using System;
using System.Dynamic;

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
        private TimeSpan ParseInterval(string interval)
        {
            if (interval != null)
            {
                string inv = interval.ToLower().Trim();
                if (inv.EndsWith("s")) return TimeSpan.FromSeconds(int.Parse(inv.Substring(0, inv.Length - 1)));
                if (inv.EndsWith("m")) return TimeSpan.FromMinutes(int.Parse(inv.Substring(0, inv.Length - 1)));
                if (inv.EndsWith("h")) return TimeSpan.FromHours(int.Parse(inv.Substring(0, inv.Length - 1)));
            }
            return TimeSpan.FromSeconds(10);
        }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Http { get; set; }
        public string Tcp { get; set; }
        public string Script { get; set; }
        public bool tlsSkipVerify { get; set; }
        public string Interval { get; set; }
        public TimeSpan IntervalSpan { get { return ParseInterval(Interval); } }
        public string Timeout { get; set; }
        public TimeSpan TimeoutSpan { get { return ParseInterval(Timeout); } }
        public string Notes { get; set; }

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
    }
}
