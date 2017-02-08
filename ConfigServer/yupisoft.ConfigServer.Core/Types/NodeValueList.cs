using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace yupisoft.ConfigServer.Core.Types
{
    public class NodeValueList : NodeValue
    {
        public List<Node> ChildNodes { get; set; }
    }
}
