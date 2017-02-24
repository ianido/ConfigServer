
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace yupisoft.ConfigServer.Client
{
    public class ApiSingleResult<T> : ApiActionResult
    {
        public T Item { get; set; }
    }
}
