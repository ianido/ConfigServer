using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace yupisoft.ConfigServer.Core
{

    public class NodeConfigSection
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public string Address { get; set; }        
    }

    public class ClusterConfigSection
    {
        public string OwnNodeName { get; set; }
        public List<NodeConfigSection> Nodes { get; set; }

        public ClusterConfigSection()
        {
            Nodes = new List<NodeConfigSection>();
        } 

    }
}
