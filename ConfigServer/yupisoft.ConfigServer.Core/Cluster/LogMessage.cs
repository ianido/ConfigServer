using System;
using System.Collections.Generic;
using System.Text;

namespace yupisoft.ConfigServer.Core.Cluster
{
    public class LogMessage
    {
        public DateTime Created { get; set; }
        public long LogId { get; set; }
        public bool Full { get; set; }
        public string TenantId { get; set; }
        public string Entity { get; set; }
        public string JsonDiff { get; set; }
    }
}
