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
        [Route("api/[controller]/set")]
        public IActionResult Set([FromBody]TNode node)
        {
            _logger.LogTrace("Success");
            ApiActionResult result = new ApiActionResult();
            result.messages.Add(new ApiResultMessage() { MessageType = ApiResultMessage.MessageTypeValues.Success });
            return result;
        }

        [HttpGet]
        [Route("api/[controller]/get/{path}")]
        public IActionResult Get(string path)
        {
            _logger.LogTrace("Success");
            ApiSingleResult<object> result = new ApiSingleResult<object>();            
            result.Item = _cfg.Get(path); 
            return result;
        }

        [HttpGet]
        [Route("api/[controller]/test")]
        public IActionResult Test()
        {
            _logger.LogTrace("Success");
            ApiSingleResult<TNode> result = new ApiSingleResult<TNode>();
            result.Item = null;
            return result;
        }       
    }
}
