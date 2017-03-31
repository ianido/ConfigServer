

using System.Collections.Generic;

namespace yupisoft.ConfigServer.Core.Cluster
{
    public class Node
    {
        private bool _inuse = false;
        public NodeConfigSection NodeConfig { get; set; }
        public bool Active { get; set; }
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
        public int InUseCycles { get; set; }
        public int InUseMaxCycles { get; set; }
        public string Id { get; set; }
        public string Address { get; set; }
        public bool Self { get; set; }
        public void ResetLife()
        {
            Life = 2000;
        }

        public Node()
        {
            InUseMaxCycles = 20;
            Life = 2000;
        }
    }
}
