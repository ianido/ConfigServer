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

        public ConfigController(ILogger<ConfigController> logger)
        {
            _logger = logger;            
        }
        
        [HttpPost]
        [Route("api/[controller]/set")]
        public IActionResult Set([FromBody]TNode node)
        {
            _logger.LogWarning("Set Invoked");
            ApiActionResult result = new ApiActionResult();
            result.messages.Add(new ApiResultMessage() { MessageType = ApiResultMessage.MessageTypeValues.Success });
            return result;
        }

        [HttpGet]
        [Route("api/[controller]/get/{path}")]
        public IActionResult Get(string path)
        {
            ApiSingleResult<TNode> result = new ApiSingleResult<TNode>();
            result.Item = null;
            return result;
        }

        [HttpGet]
        [Route("api/[controller]/test")]
        public IActionResult Test()
        {
            ApiSingleResult<TNode> result = new ApiSingleResult<TNode>();
            result.Item = null;
            return result;
        }       
    }
}
