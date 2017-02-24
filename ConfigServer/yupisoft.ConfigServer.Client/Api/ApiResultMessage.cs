using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace yupisoft.ConfigServer.Client
{
    public class ApiResultMessage
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum MessageTypeValues : int
        {
            None = 0,
            Validation = 1,
            Error = 2,
            NotFound = 3,
            Duplicated = 4,
            Success = 5,
        }

        public MessageTypeValues MessageType { get; set; }
        public string Message { get; set; }
        public string MessageCode { get; set; }
        public string Reference { get; set; }
    }
}
