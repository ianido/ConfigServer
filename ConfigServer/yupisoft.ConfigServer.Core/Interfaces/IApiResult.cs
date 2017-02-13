
using System.Collections.Generic;

namespace yupisoft.ConfigServer.Core
{
    public interface IApiResult 
    {
        int RequestId { get; set; }
        ApiResultMessage.MessageTypeValues Result { get; }
        IList<ApiResultMessage> messages { get; }
    }
}
