using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Cors;
using yupisoft.ConfigServer.Core;

namespace yupisoft.ConfigServer.Controllers
{
    
    [EnableCors("AllowAll")]
    public class ConfigController : Controller
    {
        private ILogger _logger { get; set; }
        private ConfigServerManager _cfg { get; set; }

        public ConfigController(ILogger<ConfigController> logger, ConfigServerManager configManager)
        {
            _logger = logger;
            _cfg = configManager;
        }
        
        [HttpPost]
        [Route("api/{tenantId}/[controller]/set")]
        public IActionResult Set(int tenantId, [FromBody]JNode node)
        {
            ApiActionResult result = new ApiActionResult();
            try
            {
                bool success = _cfg.Set(node, tenantId);
                if (success) result.messages.Add(new ApiResultMessage() { MessageType = ApiResultMessage.MessageTypeValues.Success });
                    else result.messages.Add(new ApiResultMessage() { MessageType = ApiResultMessage.MessageTypeValues.Error });
                _logger.LogTrace("");
            }
            catch (Exception ex)
            {
                _logger.LogTrace("Error:" + ex.ToString());
                result.messages.Add(new ApiResultMessage() { Message = ex.Message, MessageType = ApiResultMessage.MessageTypeValues.Error });
            }
            return result;
        }

        [HttpGet]
        [Route("api/{tenantId}/[controller]/get/{path}")]
        public IActionResult Get(int tenantId, string path)
        {
            ApiSingleResult<object> result = new ApiSingleResult<object>();
            try
            {
                result.Item = _cfg.Get(path, tenantId);
                if (result.Item == null) result.messages.Add(new ApiResultMessage() { MessageType = ApiResultMessage.MessageTypeValues.NotFound });
                 else result.messages.Add(new ApiResultMessage() { MessageType = ApiResultMessage.MessageTypeValues.Success });

                _logger.LogTrace("");
            }
            catch (Exception ex)
            {
                _logger.LogTrace("Error:" + ex.ToString());
                result.messages.Add(new ApiResultMessage() { Message = ex.Message, MessageType = ApiResultMessage.MessageTypeValues.Error });
            }
            return result;

        }

        [HttpGet]
        [Route("api/{tenantId}/[controller]/node/{entity}/{path}")]
        public IActionResult GetRaw(int tenantId, string entity, string path)
        {
            ApiSingleResult<object> result = new ApiSingleResult<object>();
            try
            {
                result.Item = _cfg.GetRaw(path, entity, tenantId);
                if (result.Item == null) result.messages.Add(new ApiResultMessage() { MessageType = ApiResultMessage.MessageTypeValues.NotFound });
                else result.messages.Add(new ApiResultMessage() { MessageType = ApiResultMessage.MessageTypeValues.Success });

                _logger.LogTrace("");
            }
            catch (Exception ex)
            {
                _logger.LogTrace("Error:" + ex.ToString());
                result.messages.Add(new ApiResultMessage() { Message = ex.Message, MessageType = ApiResultMessage.MessageTypeValues.Error });
            }
            return result;
        }

        [HttpGet]
        [Route("api/[controller]/test")]
        public IActionResult Test()
        {
            _logger.LogTrace("Success");
            ApiSingleResult<JNode> result = new ApiSingleResult<JNode>();
            result.Item = null;
            return result;
        }       
    }
}
