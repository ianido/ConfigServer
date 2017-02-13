using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace yupisoft.ConfigServer.Core
{
    public interface IApiRequest
    {
        int RequestId { get; set; } // This is the echo id of the request to handle async request
    }
}
