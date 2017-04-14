using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Cors;
using yupisoft.ConfigServer.Core;
using yupisoft.ConfigServer.Core.Cluster;
using Microsoft.AspNetCore.Authentication.Hmac;
using Microsoft.AspNetCore.Authorization;

namespace yupisoft.ConfigServer.Controllers
{
    
    [EnableCors("AllowAll")]
    [Authorize(Policy = "Cluster")]
    public class ClusterController : Controller
    {
        private ILogger _logger { get; set; }
        private ClusterManager _clsManager { get; set; }


        public ClusterController(ILogger<ClusterController> logger, ClusterManager clsManager)
        {
            _logger = logger;
            _clsManager = clsManager;
        }
        /*
        [HttpPost]
        [Route("api/[controller]/heartbeat")]        
        public IActionResult Heartbeat([FromBody]HeartBeatMessageRequest msg)
        {
            
            ApiSingleResult<HeartBeatMessageResponse> result = new ApiSingleResult<HeartBeatMessageResponse>();
            try
            {
                bool success = true;
                if (success) result.messages.Add(new ApiResultMessage() { MessageType = ApiResultMessage.MessageTypeValues.Success });
                    else result.messages.Add(new ApiResultMessage() { MessageType = ApiResultMessage.MessageTypeValues.Error });
                result.Item = _clsManager.ProcessHeartBeatRequest(msg);
            }
            catch (Exception ex)
            {
                _logger.LogTrace("Error:" + ex.ToString());
                result.messages.Add(new ApiResultMessage() { Message = ex.Message, MessageType = ApiResultMessage.MessageTypeValues.Error });
            }
            return result;
        }*/

        [HttpPost]
        [Route("api/[controller]/heartbeat")]
        public IActionResult Heartbeat([FromBody]HeartBeatMessage msg)
        {

            ApiSingleResult<HeartBeatMessage> result = new ApiSingleResult<HeartBeatMessage>();
            try
            {
                bool success = true;
                if (success) result.messages.Add(new ApiResultMessage() { MessageType = ApiResultMessage.MessageTypeValues.Success });
                else result.messages.Add(new ApiResultMessage() { MessageType = ApiResultMessage.MessageTypeValues.Error });
                result.Item = _clsManager.ReceiveHeartBeat(msg);
            }
            catch (Exception ex)
            {
                _logger.LogTrace("Error:" + ex.ToString());
                result.messages.Add(new ApiResultMessage() { Message = ex.Message, MessageType = ApiResultMessage.MessageTypeValues.Error });
            }
            return result;
        }


    }
}
