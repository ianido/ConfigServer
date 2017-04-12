using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace yupisoft.ConfigServer.Core
{

    public class NodeConfigSection
    {
        public string Id { get; set; }
        public bool Enabled { get; set; }
        public string Address { get; set; }
    }

    public class ClusterConfigMonitoringSection
    {
        public string Mode { get; set; }
        public int Interval { get; set; }
        public int MaxAttempts { get; set; }
        public int SkipAttemptsOnFail { get; set; }
        public int NodesLife { get; set; }
    }

    public class ClusterConfigSection
    {
        public string OwnNodeName { get; set; }
        public string OwnNodeUrl { get; set; }
        public HmacAuthenticationOptions Security { get; set; }
        public ClusterConfigMonitoringSection Monitoring { get; set; }
        public List<NodeConfigSection> Nodes { get; set; }
        public ClusterConfigSection()
        {
            Nodes = new List<NodeConfigSection>();
            Monitoring = new ClusterConfigMonitoringSection();
            Security = new HmacAuthenticationOptions();
        } 



    }
}
