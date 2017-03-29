using System;
using System.Collections.Generic;
using System.Text;

namespace yupisoft.ConfigServer.Core.Cluster
{
    public enum HeartBeartCommand
    {
        HeartBeatRequest,
        HeartBeatResponse,
        SyncRequest,
        SyncResponse,
        InUse,
        Unknow
    }
    public class HeartBeatMessageRequest
    {
        public string NodeId { get; set; }
        public HeartBeartCommand Command { get; set; }
        public long LastLogId { get; set; }
        public DateTime Created { get; set; }
        public List<LogMessage> Log { get; set; }
        public NodeConfigSection[] Nodes { get; set; }
        public HeartBeatMessageRequest()
        {
            Created = DateTime.UtcNow;
            Log = null;
            Nodes = new NodeConfigSection[0];
            LastLogId = 0;
        }
    }

    public class HeartBeatMessageResponse
    {
        public HeartBeartCommand Command { get; set; }
        public string NodeId { get; set; }

        public long LastLogId { get; set; }
        public List<LogMessage> Log { get; set; }
        public DateTime Created { get; set; }
    }
}
