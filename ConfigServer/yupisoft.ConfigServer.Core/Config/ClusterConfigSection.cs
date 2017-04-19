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
        public bool HeartBeat { get; set; }
        public string Uri { get; set; }
        public string WANUri { get; set; }
        public string Mode { get; set; }
        public string DataCenter { get; set; }

        public void CopyFrom(NodeConfigSection node)
        {
            this.Enabled = node.Enabled;
            this.Mode = node.Mode;
            this.Uri = node.Uri;
            this.WANUri = node.WANUri;
            this.DataCenter = node.DataCenter;
            this.HeartBeat = node.HeartBeat;
        }
        public string Serialize() {
            if (string.IsNullOrEmpty(Mode)) Mode = "server";
            if (Mode!="server" && Mode != "client") Mode = "server";
            return Id.ToString() + "|" + (Enabled ? "1" : "0") + "|" + Uri + "|" + WANUri + "|" + Mode[0]+ "|" + (HeartBeat ? "1" : "0") + "|" + DataCenter;
        }
        public static NodeConfigSection Deserialize(string serialized)
        {
            NodeConfigSection node = new NodeConfigSection();
            if (string.IsNullOrEmpty(serialized)) return null;
            var parts = serialized.Split('|');
            if (parts.Length < 6) return null;
            node.Id = parts[0];
            node.Enabled = parts[1] == "1" ? true : false;
            node.Uri = parts[2];
            node.WANUri = parts[3];
            if (parts[4] == "s") node.Mode = "server";
            if (parts[4] == "c") node.Mode = "client";
            node.HeartBeat = parts[5] == "1" ? true : false;
            node.DataCenter = parts[6];
            return node;
        }
        public NodeConfigSection()
        {
            Mode = "server";
            Enabled = false;
            HeartBeat = true;
            Uri = "";
            WANUri = "";
        }
    }

    public class ClusterConfigMonitoringSection
    {
        public int DataCenterMaxInterval { get; set; }
        public int DataCenterMinInterval { get; set; }
        public int Interval { get; set; }
        public int MaxAttempts { get; set; }
        public int SkipAttemptsOnFail { get; set; }
        public int NodesLife { get; set; }
    }

    public class ClusterConfigSection
    {
        public string OwnNodeName { get; set; }
        public string OwnNodeUrl { get; set; }
        public string Balancer { get; set; }
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
