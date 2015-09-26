using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace nsteam.ConfigServer.Types
{
    public class TreeController : ApiController 
    {
        private ILoggerService _logger;

        private static ConfigRepository _repo = null;

        private ConfigRepository repo
        {
            get
            {
                if (_repo == null)
                    _repo = new ConfigRepository(_logger);
                return _repo;
            }
        }

        public TreeController(ILoggerService logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("api/{name}/node")]
        public HttpResponseMessage GetNode(string name)
        {
            return GetNode(name, "/");
        }

        [HttpGet]
        [Route("api/{name}/node/{path}")]
        public HttpResponseMessage GetNode(string name, string path)
        {
            try
            {
                dynamic r = repo.GetNode(name, path);
                return Request.CreateResponse(HttpStatusCode.OK, (object)r);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [HttpGet]
        [Route("api/{name}/tree/{path}")]
        public HttpResponseMessage GetTree(string name, string path)
        {
            try
            {
                dynamic r = repo.GetTree(name, path);
                return Request.CreateResponse(HttpStatusCode.OK, (object)r);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [HttpGet]
        [Route("api/{name}/tree")]
        public HttpResponseMessage GetTree(string name)
        {
            return GetTree(name, "/");
        }

        [HttpPost]
        [Route("api/{name}/node")]
        public HttpResponseMessage SetNode(string name, [FromBody]TNode node)
        {
            try
            {
                repo.SetNode(name, node);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

    }
}
