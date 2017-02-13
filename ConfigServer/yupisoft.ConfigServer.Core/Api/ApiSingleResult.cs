
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace yupisoft.ConfigServer.Core
{
    public class ApiSingleResult<T> : ApiActionResult, ISingleResult<T>
    {

        public T Item { get; set; }

        public ApiSingleResult(T item, int requestId)
        {
            Item = item;
            RequestId = requestId;
            messages = new List<ApiResultMessage>();
        }

        public ApiSingleResult(int requestId)
        {
            RequestId = requestId;
            messages = new List<ApiResultMessage>();
        }

        public ApiSingleResult()
        {
            messages = new List<ApiResultMessage>();
        }
    }
}
