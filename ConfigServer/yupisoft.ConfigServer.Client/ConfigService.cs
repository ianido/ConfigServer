using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace yupisoft.ConfigServer.Client
{
    public class ConfigService
    {
        private static HttpClient _client = null;
        private string _serveraddr = "";
        private string _rootnode = "";
        private string _appId = "";
        private string _apiKey = "";
        private string _tenantId;
        private HttpClient client
        {
            get
            {
                if (_client == null) _client = new HttpClient();
                return _client;
            }
        }

        #region Constructors
#if NET452
        public ConfigService()
        {
            _serveraddr = ClientConfigSection.GetSettings().Server;
            _rootnode = ClientConfigSection.GetSettings().BaseNode;
            _tenantId = ClientConfigSection.GetSettings().TenantId;
            _appId = ClientConfigSection.GetSettings().APPId;
            _apiKey = ClientConfigSection.GetSettings().APIKey;
            if (!_serveraddr.EndsWith("/")) _serveraddr += "/";
        }
#endif


        public ConfigService(string serveraddr, string rootnode, string tenantId, string AppId, string APIKey)
        {
            _appId = AppId;
            _apiKey = APIKey;
            _tenantId = tenantId;
            _serveraddr = serveraddr;
            _rootnode = rootnode;
            if (!_serveraddr.EndsWith("/")) _serveraddr += "/";
        }

        #endregion

        #region Private

        private string GetNode(string path)
        {
            var message = new HttpRequestMessage(); 
            message.Method = HttpMethod.Get;
            message.RequestUri = new Uri(_serveraddr + "api/"+_tenantId+"/config/get/" + _rootnode + ((!string.IsNullOrEmpty(_rootnode) && !string.IsNullOrEmpty(path)) ? "." : "") + path);
            var response = client.SendAsync(message, new CancellationToken(), _appId, _apiKey).Result;
            string json = response.Content.ReadAsStringAsync().Result;
            return json;
        }

        private string GetRawNode(string path, string entity)
        {
            if (string.IsNullOrEmpty(entity)) entity = "@default";
            var message = new HttpRequestMessage();
            message.Method = HttpMethod.Get;
            message.RequestUri = new Uri(_serveraddr + "api/" + _tenantId + "/config/node/" + entity + "/" + _rootnode + ((!string.IsNullOrEmpty(_rootnode) && !string.IsNullOrEmpty(path)) ? "." : "") + path);
            var response = client.SendAsync(message, new CancellationToken(), _appId, _apiKey).Result;
            string json = response.Content.ReadAsStringAsync().Result;
            return json;
        }
        
        private string SaveNode<T>(T node)
        {
            string str = JsonConvert.SerializeObject(node);
            StringContent content = new StringContent(str, Encoding.UTF8, "application/json");

            var message = new HttpRequestMessage();
            message.Method = HttpMethod.Post;
            message.Content = content;
            message.RequestUri = new Uri(_serveraddr + "api/" + _tenantId + "/config/set");
            var response = client.SendAsync(message, new CancellationToken(), _appId, _apiKey).Result;
            string json = response.Content.ReadAsStringAsync().Result;
            return json;
        }

        #endregion

        public T Get<T>(string path, string entity = null)
        {
            string content = null;
            if (typeof(T).Name.StartsWith("TNode") && (typeof(T).Namespace == "yupisoft.ConfigServer.Client"))
                content = GetRawNode(path, entity);
            else 
                content = GetNode(path);
            if (content == null) return default(T);
            var result = JsonConvert.DeserializeObject<ApiSingleResult<T>>(content);
            if (result == null) return default(T);
            return result.Item;
        }

        public bool Set(TNode node)
        {
            var content = SaveNode<TNode>(node);
            var result = JsonConvert.DeserializeObject<ApiActionResult>(content);
            if (result == null) return false;
            return result.Result == ApiResultMessage.MessageTypeValues.Success;
        }

        public bool Set<T>(TNode<T> node)
        {
            var content = SaveNode<TNode<T>>(node);
            var result = JsonConvert.DeserializeObject<ApiActionResult>(content);
            if (result == null) return false;
            return result.Result == ApiResultMessage.MessageTypeValues.Success;
        }

        
    }
}
