

using System.Collections.Generic;

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
            Self = true;
            Life = 2;
        }
    }
}
