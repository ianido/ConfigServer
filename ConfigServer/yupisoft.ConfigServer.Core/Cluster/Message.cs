using System;
using System.Collections.Generic;
using System.Text;


namespace yupisoft.ConfigServer.Core.Cluster
{
    //public enum HeartBeartCommand
    //{
    //    HeartBeatRequest,
    //    HeartBeatResponse,
    //    SyncRequest,
    //    SyncResponse,
    //    FullSyncRequest,
    //    FullSyncResponse,
    //    Abort,
    //    Unknow
    //}

    //public enum HeartBeartCommandResult
    //{
    //    Success,
    //    Abort,
    //    InUse,
    //    Error,
    //    InvalidSignature
    //}

    //public class HeartBeatMessageRequest 
    //{
    //    public string NodeId { get; set; }
    //    public Node.NodeMode NodeMode { get; set; }
    //    public DateTime NodeAliveSince { get; set; }
    //    public KeyValuePair<string, string>[] DataHash { get; set; }
    //    public HeartBeartCommand Command { get; set; }
    //    public long LastLogId { get; set; }
    //    public DateTime LastLogDate { get; set; }
    //    public DateTime Created { get; set; }
    //    public List<LogMessage> Log { get; set; }
    //    public NodeConfigSection[] Nodes { get; set; }
    //    public HeartBeatMessageRequest()
    //    {
    //        Created = DateTime.UtcNow;
    //        Log = new List<LogMessage>();
    //        Nodes = new NodeConfigSection[0];
    //        LastLogId = 0;
    //    }
    //}

    //public class HeartBeatMessageResponse 
    //{
    //    public DateTime Created { get; set; }
    //    public DateTime NodeAliveSince { get; set; }
    //    public long LastLogId { get; set; }
    //    public string NodeId { get; set; }
    //    public HeartBeartCommand Command { get; set; }
    //    public List<LogMessage> Log { get; set; }
    //    public HeartBeartCommandResult Result { get; set; }
    //}

    public enum HeartBeartType
    {
        Unknow,
        HeartBeatRequest,
        HeartBeatResponse,
        HeartBeatUpdateResponse,
        UpdateRequest,
        UpdateResponse,
        Abort        
    }

    public class TenantHash
    {
        public string Id { get; set; }
        public string Hash { get; set; }
    }

    public class HeartBeatMessage
    {
        public HeartBeartType MessageType { get; set; }
        public string NodeId { get; set; }
        public Node.NodeMode NodeMode { get; set; }
        public DateTime NodeAliveSince { get; set; }
        public long LastLogId { get; set; }
        public DateTime LastLogDate { get; set; }
        public DateTime Created { get; set; }
        public List<LogMessage> Log { get; set; }
        public string[] Nodes { get; set; }
        public TenantHash[] DataHash { get; set; }

        public HeartBeatMessage()
        {
            Log = new List<LogMessage>();
            Nodes = new string[0];
        }
    }
}
