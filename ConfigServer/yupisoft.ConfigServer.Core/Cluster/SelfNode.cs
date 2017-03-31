

using System;
using System.Collections.Generic;
using System.Linq;

namespace yupisoft.ConfigServer.Core.Cluster
{
    public enum SelfNodeStatus
    {
        Normal,
        Unsyncronized,
    }
    public class SelfNode : Node
    {
        public SelfNodeStatus Status { get; set; }
        public List<LogMessage> LogMessages { get; set; }

        public SelfNode() : base()
        {
            Status = SelfNodeStatus.Normal;
            LogMessages = new List<LogMessage>();
            Self = true;
            Life = 2;
        }

        public long LastLogId {
            get
            {
                if (LogMessages.Count > 0)
                    return LogMessages.Last().LogId;
                return 0;
            }            
        }

        public DateTime LastLogDate
        {
            get
            {
                if (LogMessages.Count > 0)
                    return LogMessages.Last().Created;
                return DateTime.MaxValue;
            }
        }
    }
}
