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
        FullSyncRequest,
        FullSyncResponse,
        InUse,
        Unknow
    }

    public enum HeartBeartCommandResult
    {
        Success,
        Abort,
        Error
    }
    public class HeartBeatMessageRequest
    {
        public string NodeId { get; set; }
        public DateTime NodeAliveSince { get; set; }
        public KeyValuePair<int, string>[] DataHash { get; set; }
        public HeartBeartCommand Command { get; set; }
        public long LastLogId { get; set; }
        public DateTime LastLogDate { get; set; }
        public DateTime Created { get; set; }
        public List<LogMessage> Log { get; set; }
        public NodeConfigSection[] Nodes { get; set; }
        public HeartBeatMessageRequest()
        {
            Created = DateTime.UtcNow;
            Log = new List<LogMessage>();
            Nodes = new NodeConfigSection[0];
            LastLogId = 0;
        }
    }

    public class HeartBeatMessageResponse
    {
        public string NodeId { get; set; }
        public long LastLogId { get; set; }
        public HeartBeartCommand Command { get; set; }
        public DateTime NodeAliveSince { get; set; }
        public List<LogMessage> Log { get; set; }
        public DateTime Created { get; set; }
        public HeartBeartCommandResult Result { get; set; }
    }
}
