using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace yupisoft.ConfigServer.Core
{
    public class ConfigurationChanger
    {
        public string _AppSettings { get; private set; }
        public ConfigurationChanger(string appSettings)
        {
            _AppSettings = appSettings;
        }
        public void DisableClusterNode(string Id)
        {
            var jsonString = File.ReadAllText(_AppSettings);
            JToken jsonObject = JsonConvert.DeserializeObject<JToken>(jsonString);
            JToken toRemove = jsonObject.SelectToken("ConfigServer.Nodes[?(@.Id == '"+ Id +"')]");
            if (toRemove != null)
            {
                toRemove["Enabled"] = false;
                var modifiedJsonString = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
                File.WriteAllText(_AppSettings, modifiedJsonString);
            }
        }
    }
}
