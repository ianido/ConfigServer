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
        private IConfigurationService _config;
        private ILoggerService _logger;
        private string _repofilename;
        private static ConfigRepository _repo = null;

        private ConfigRepository repo
        {
            get
            {
                if (_repo == null)
                    _repo = new ConfigRepository(_logger, _repofilename);
                return _repo;
            }
        }

        public TreeController(IConfigurationService config, ILoggerService logger)
        {
            _logger = logger;
            _config = config;
            _repofilename = config.GetSetting("configfile");
        }

        [HttpGet]
        [Route("api/node/")]
        public HttpResponseMessage GetNode()
        {
            return GetNode("/");
        }

        [HttpGet]
        [Route("api/node/{path}")]
        public HttpResponseMessage GetNode(string path)
        {
            try
            {
                dynamic r = repo.GetNode(path);
                return Request.CreateResponse(HttpStatusCode.OK, (object)r);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [HttpGet]
        [Route("api/tree/{path}")]
        public HttpResponseMessage GetTree(string path)
        {
            try
            {
                dynamic r = repo.GetTree(path);
                return Request.CreateResponse(HttpStatusCode.OK, (object)r);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [HttpGet]
        [Route("api/tree/")]
        public HttpResponseMessage GetTree()
        {
            return GetTree("/");
        }

        [HttpPost]
        [Route("api/node/")]
        public HttpResponseMessage SetNode([FromBody]TNode node)
        {
            try
            {
                repo.SetNode(node);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

    }
}
