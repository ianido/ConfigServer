

using System.Collections.Generic;

namespace yupisoft.ConfigServer.Core.Cluster
{
    public class Node
    {
        public enum NodeMode
        {            
            Server, /* Server mode: Server nodes heartbeat other server nodes and receive hartbeat, and replicate changes and discovery server nodes between them */
            Client  /* Client mode: Client nodes heartbeat only one server node (the highest priority), 
                       if it fail then decrease the priority of that node and the next heartbeat will be performed against another node,
                       and replicate changes from server and discovery nodes between them.
                       Client nodes do not receive heartbeats from server nodes.*/
        }
        private bool _inuse = false;
        public NodeConfigSection NodeConfig { get; private set; }
        public bool Active { get; set; }
        public bool? LastChoosed { get; set; }
        public bool LastCheckActive { get; set; }
        public int SkipAttempts { get; set; }
        public int Attempts { get; set; }
        public int Life { get; set; }
        public bool InUse {
            get {
                if (_inuse) InUseCycles++;
                if (InUseCycles > InUseMaxCycles)
                {
                    _inuse = false;
                    InUseCycles = 0;
                }
                return _inuse;
            }
            set {
                if (value) InUseCycles = 0;
                _inuse = value;
            }
        }
        public int Priority { get; set; }
        public int InUseCycles { get; set; }
        public int InUseMaxCycles { get; set; }
        public string DataCenter { get { return NodeConfig.DataCenter; } }
        public string Id { get { return NodeConfig.Id; } }
        public string Uri { get { return NodeConfig.Uri; } }
        public string WANUri { get { return NodeConfig.WANUri; } }
        public bool Self { get; set; }
        public NodeMode Mode
        {
            get
            {
                if (NodeConfig.Mode.ToLower() == "server") return NodeMode.Server;
                if (NodeConfig.Mode.ToLower() == "client") return NodeMode.Client;
                return NodeMode.Server;
            }
        }
        public void ResetLife()
        {
            Life = 2000;
        }
        public Node(NodeConfigSection config)
        {
            LastChoosed = null;
            Active = true;
            LastCheckActive = true;
            NodeConfig = config;
            InUseMaxCycles = 20;
            Life = 2000;
            Priority = 9999;
        }
    }
}
