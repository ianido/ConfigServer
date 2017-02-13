using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace yupisoft.ConfigServer.Core
{
    public interface ISingleResult<T>
    {
        T Item { get; set; }
    }
}
