using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;

namespace nsteam.ConfigServer.Client
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
            _serveraddr = ClientConfigSection.GetSettings().Server;
            _rootnode = ClientConfigSection.GetSettings().BaseNode;
        }

        public ConfigService(string serveraddr)
        {
            _serveraddr = serveraddr;
        }

        public ConfigService(string serveraddr, string rootnode)
        {
            _serveraddr = serveraddr;
            _rootnode = rootnode;
        }

        #endregion

        #region Private

        private string GetNodeResult(string path)
        {
            var message = new HttpRequestMessage();
            message.Version = HttpVersion.Version11;
            message.Method = HttpMethod.Get;
            message.RequestUri = new Uri(_serveraddr + "api/node/" + _rootnode + ((!string.IsNullOrEmpty(_rootnode) && !string.IsNullOrEmpty(path)) ? "." : "") + path);
            var response = client.SendAsync(message).Result;
            //var response = client.GetAsync(_serveraddr + "api/node/" + _rootnode + ((!string.IsNullOrEmpty(_rootnode) && !string.IsNullOrEmpty(path)) ? "." : "") + path).Result;
            string json = response.Content.ReadAsStringAsync().Result;
            return json;
        }

        private string GetNodeProcessedResult(string path, bool includeObjectInfo)
        {

            var message = new HttpRequestMessage();
            message.Version = HttpVersion.Version11;
            message.Method = HttpMethod.Get;
            message.RequestUri = new Uri(_serveraddr + "api/tree/" + _rootnode + ((!string.IsNullOrEmpty(_rootnode) && !string.IsNullOrEmpty(path)) ? "." : "") + path);
            var response = client.SendAsync(message).Result;
            //var response = client.GetAsync(_serveraddr + "api/tree/" + _rootnode + ((!string.IsNullOrEmpty(_rootnode) && !string.IsNullOrEmpty(path)) ? "." : "") + path).Result;
            string json = response.Content.ReadAsStringAsync().Result;

            if (!includeObjectInfo)
            {
                // Remove object info from Json
                string objectinfomath = @"(,?\s*"")(objectinfo)(""\s*)(:)(\s*{)";
                Match match = Regex.Match(json, objectinfomath, RegexOptions.IgnoreCase);
                while (match.Success)
                {
                    int nbr = 1;
                    int i = match.Index + match.Length;
                    while (nbr > 0)
                    {
                        if (json[i] == '{') nbr++;
                        if (json[i] == '}') nbr--;
                        i++;
                    }
                    json = json.Remove(match.Index, i - match.Index);
                    match = Regex.Match(json, objectinfomath, RegexOptions.IgnoreCase);
                }
            }

            return json;
        }

        #endregion

        #region Obtain Nodes Processeds

        public T GetTree<T>(string path)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(GetNodeProcessedResult(path, false));
            //return Json.Decode<T>(GetNodeProcessedResult(path, false));
        }

        public T GetTree<T>()
        {
            return GetTree<T>("");
        }

        public dynamic GetTree(string path)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(GetNodeProcessedResult(path, false));
            //return Json.Decode(GetNodeProcessedResult(path, false));
        }

        public dynamic GetTree()
        {
            return GetTree("");
        }
        
        public T GetTree<T>(string path, bool includeObjectInfo)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(GetNodeProcessedResult(path, includeObjectInfo));
            //return Json.Decode<T>(GetNodeProcessedResult(path, includeObjectInfo));
        }

        public dynamic GetTree(string path, bool includeObjectInfo)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(GetNodeProcessedResult(path, includeObjectInfo));
            //return Json.Decode(GetNodeProcessedResult(path, includeObjectInfo));
        }

        #endregion

        #region Obtain Crude Nodes

        public TNode<T> GetNode<T>(string path)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<TNode<T>>(GetNodeResult(path));
            //return Json.Decode<TNode<T>>(GetNodeResult(path));
        }

        public TNode<T> GetNode<T>()
        {
            return GetNode<T>("");
        }

        public TNode GetNode(string path)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<TNode>(GetNodeResult(path));
            //return Json.Decode<TNode>(GetNodeResult(path));
        }

        public TNode GetNode()
        {
            return GetNode("");
        }

        #endregion

        public void SaveNode(TNode obj)
        {
            //string str = Json.Encode(obj);
            string str = Newtonsoft.Json.JsonConvert.SerializeObject(obj);

            StringContent content = new System.Net.Http.StringContent(str, Encoding.UTF8, "application/json");

            var message = new HttpRequestMessage();
            message.Version = HttpVersion.Version11;
            message.Method = HttpMethod.Post;
            message.Content = content;                 
            message.RequestUri = new Uri(_serveraddr + "api/node");
            var response = client.SendAsync(message).Result;



            //var response = client.PostAsync(_serveraddr + "api/node", content).Result;

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                throw new ApplicationException(response.Content.ReadAsStringAsync().Result);
        }


    }
}
