using System;
using System.Collections.Generic;
using System.Text;

namespace yupisoft.ConfigServer.Core.Cluster
{
    public class HeartBeatMessageRequest
    {
        public long Term { get; set; }
        public DateTime Created { get; set; }
        public object Data { get; set; }
    }

    public class HeartBeatMessageResponse
    {
        public long Term { get; set; }
        public DateTime Created { get; set; }
        public object Data { get; set; }
    }
}
