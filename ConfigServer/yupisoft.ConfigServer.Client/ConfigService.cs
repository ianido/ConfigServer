using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;


namespace yupisoft.ConfigServer.Client
{
    public class ConfigService
    {
        private static HttpClient _client = null;
        private string _serveraddr = "";
        private string _rootnode = "";
        private HttpClient client
        {
            get
            {
                if (_client == null) _client = new HttpClient();
                return _client;
            }
        }

        #region Constructors

        public ConfigService()
        {
#if NET452
            _serveraddr = ClientConfigSection.GetSettings().Server;
            _rootnode = ClientConfigSection.GetSettings().BaseNode;
            if (!_serveraddr.EndsWith("/")) _serveraddr += "/";
#endif
        }

        public ConfigService(string serveraddr)
        {
            _serveraddr = serveraddr;
            if (!_serveraddr.EndsWith("/")) _serveraddr += "/";
        }

        public ConfigService(string serveraddr, string rootnode)
        {
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
            message.RequestUri = new Uri(_serveraddr + "api/config/get/" + _rootnode + ((!string.IsNullOrEmpty(_rootnode) && !string.IsNullOrEmpty(path)) ? "." : "") + path);
            var response = client.SendAsync(message).Result;
            string json = response.Content.ReadAsStringAsync().Result;
            return json;
        }

        public string SaveNode<T>(string path, T obj)
        {
            TNode<T> node = new TNode<T>() { Path = path, Value = obj };
            string str = JsonConvert.SerializeObject(node);
            StringContent content = new StringContent(str, Encoding.UTF8, "application/json");

            var message = new HttpRequestMessage();
            message.Method = HttpMethod.Post;
            message.Content = content;
            message.RequestUri = new Uri(_serveraddr + "api/config/set");
            var response = client.SendAsync(message).Result;
            string json = response.Content.ReadAsStringAsync().Result;
            return json;
        }

        #endregion

        public T Get<T>(string path)
        {
            var result = JsonConvert.DeserializeObject<ApiSingleResult<T>>(GetNode(path));
            return result.Item;
        }
        public bool Set<T>(string path, T obj)
        {
            var node = SaveNode<T>(path, obj);
            var result = JsonConvert.DeserializeObject<ApiActionResult>(node);
            return result.Result == ApiResultMessage.MessageTypeValues.Success;
        }

    }
}
