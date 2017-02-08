using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using yupisoft.ConfigServer.Core.Enum;

namespace yupisoft.ConfigServer.Core.Types
{


    public class Node
    {
        public string Name { get; private set; }
        public NodeType Type { get; private set; }
        public object Value { get; private set; }

        protected Node(NodeType type, object value)
        {
            Value = value;
            Type = type;
        }

        public static Node CreateNode(NodeType type)
        {
            if (type == NodeType.Boolean)
                return new Node(type, false);
            if (type == NodeType.Array)
                return new Node(type, new List<Node>());

        }

    }
}
