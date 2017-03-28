

namespace yupisoft.ConfigServer.Core.Cluster
{
    public class Node
    {
        public bool Active { get; set; }
        public string Id { get; set; }
        public string Address { get; set; }
        public bool Self { get; set; }
    }
}
