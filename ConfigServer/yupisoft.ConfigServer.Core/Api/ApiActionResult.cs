
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace yupisoft.ConfigServer.Core
{
    public class ApiActionResult: IApiResult, IActionResult
    {
        public int RequestId { get; set; }
        public IList<ApiResultMessage> messages { get; set; }

        public ApiResultMessage.MessageTypeValues Result
        {
            get
            {
                foreach (var v in messages)
                    return v.MessageType;
                return ApiResultMessage.MessageTypeValues.Success;
            }
        }

        public ApiActionResult()
        {
            messages = new List<ApiResultMessage>();
        }

        public ApiActionResult(int requestId)
        {
            RequestId = requestId;
            messages = new List<ApiResultMessage>();
        }

        public Task ExecuteResultAsync(ActionContext context)
        {
            var response = context.HttpContext.Response;
            if (Result == ApiResultMessage.MessageTypeValues.Error) response.StatusCode = (int)HttpStatusCode.InternalServerError;
            if (Result == ApiResultMessage.MessageTypeValues.NotFound) response.StatusCode = (int)HttpStatusCode.NotFound;
            if (Result == ApiResultMessage.MessageTypeValues.Success) response.StatusCode = (int)HttpStatusCode.OK;
            if (Result == ApiResultMessage.MessageTypeValues.Duplicated) response.StatusCode = (int)HttpStatusCode.BadRequest;
            if (Result == ApiResultMessage.MessageTypeValues.Validation) response.StatusCode = (int)HttpStatusCode.BadRequest;
            response.ContentType = "application/json";
            var content = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(this));
            return response.Body.WriteAsync(content, 0, content.Length);
        }
    }
}
