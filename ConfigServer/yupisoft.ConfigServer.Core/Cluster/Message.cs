using System;
using System.Collections.Generic;
using System.Text;

namespace yupisoft.ConfigServer.Core.Cluster
{
    public class HeartBeatMessageRequest
    {
        public long LogId { get; set; }
        public DateTime Created { get; set; }
        public object Data { get; set; }
        public NodeConfigSection[] Nodes { get; set; }
        public HeartBeatMessageRequest()
        {
            Created = DateTime.UtcNow;
            Data = null;
            Nodes = new NodeConfigSection[0];
            LogId = 1;
        }
    }

    public class HeartBeatMessageResponse
    {
        public long LogId { get; set; }
        public DateTime Created { get; set; }
        public object Data { get; set; }
    }
}
