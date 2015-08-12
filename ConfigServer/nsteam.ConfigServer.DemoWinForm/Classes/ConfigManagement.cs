using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace nsteam.ConfigServer.DemoWinForm
{
    public class ConfigManagement : IConfigManagement
    {
        private HttpClient _client = new HttpClient();
        private string _serveraddr = "http://localhost:9000/";
        private string _basenode = "configuration";

        public string GetNode(string path)
        {
            var response = _client.GetAsync(_serveraddr + "api/node/" + _basenode + "." + path).Result;
            string json = response.Content.ReadAsStringAsync().Result;
            return json;
        }

        public string GetTree(string path)
        {
            var response = _client.GetAsync(_serveraddr + "api/tree/" + _basenode + "." + path).Result;
            string json = response.Content.ReadAsStringAsync().Result;
            return json;
        }

        public void SetNode(string node)
        {
            StringContent content = new System.Net.Http.StringContent(node, Encoding.UTF8, "application/json");
            var result = _client.PostAsync(_serveraddr + "api/node", content).Result;
            if (result.StatusCode != System.Net.HttpStatusCode.OK)
                throw new ApplicationException(result.Content.ReadAsStringAsync().Result);
        }
    }
}
