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
        public string Mode { get; set; }
        public string Serialize() {
            if (string.IsNullOrEmpty(Mode)) Mode = "server";
            if (Mode!="server" && Mode != "client") Mode = "server";
            return Id.ToString() + "|" + (Enabled ? "1" : "0") + "|" + Address + "|" + Mode[0];
        }
        public void Deserialize(string serialized)
        {
            if (string.IsNullOrEmpty(serialized)) return;
            var parts = serialized.Split('|');
            if (parts.Length < 5) return;
            Id = parts[0];
            Enabled = parts[1] == "1" ? true : false;
            Address = parts[2];
            if (parts[3] == "s") Mode = "server";
            if (parts[3] == "c") Mode = "client";
        }
        public NodeConfigSection()
        {
            Mode = "server";
            Enabled = false;
            Address = "";
        }
    }

    public class ClusterConfigMonitoringSection
    {        
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
