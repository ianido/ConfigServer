
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace yupisoft.ConfigServer.Client
{
    public class ApiActionResult
    {
        public int RequestId { get; set; }
        public IList<ApiResultMessage> messages { get; set; }
        public ApiResultMessage.MessageTypeValues Result { get; set; }

    }
}
