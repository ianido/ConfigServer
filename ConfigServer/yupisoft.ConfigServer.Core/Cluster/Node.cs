

namespace yupisoft.ConfigServer.Core.Cluster
{
    public class Node
    {
        public bool Active { get; set; }
        public int SkipAttempts { get; set; }
        public int Attempts { get; set; }
        public int Life { get; set; }
        public bool InUse { get; set; }
        public string Id { get; set; }
        public string Address { get; set; }
        public bool Self { get; set; }

        public Node()
        {
            Life = 2;
        }
    }
}
