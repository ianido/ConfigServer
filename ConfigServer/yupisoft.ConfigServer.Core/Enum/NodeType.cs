using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace yupisoft.ConfigServer.Core.Enum
{
    public enum NodeType
    {
        String,
        Number,
        Boolean,
        Object,
        Array,
        Reference,
        Inheritance,
        Include
    }
}
