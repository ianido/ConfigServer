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

        public void UpdateClusterNode(NodeConfigSection node)
        {
            var jsonString = File.ReadAllText(_AppSettings);
            JToken jsonObject = JsonConvert.DeserializeObject<JToken>(jsonString);
            JToken toEdit = jsonObject.SelectToken("ConfigServer.Nodes[?(@.Id == '" + node.Id + "')]");
            if (toEdit != null)
            {
                toEdit["Enabled"] = node.Enabled;
                toEdit["Address"] = node.Address;
                toEdit["Mode"] = node.Mode;
                var modifiedJsonString = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
                File.WriteAllText(_AppSettings, modifiedJsonString);
            }
        }

        public void AddClusterNode(NodeConfigSection node)
        {
            var jsonString = File.ReadAllText(_AppSettings);
            JToken jsonObject = JsonConvert.DeserializeObject<JToken>(jsonString);
            JArray arTokens = (JArray)jsonObject.SelectToken("ConfigServer.Nodes");
            if (arTokens != null)
            {
                arTokens.Add(JToken.FromObject(node));
                var modifiedJsonString = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
                File.WriteAllText(_AppSettings, modifiedJsonString);
            }
        }

    }
}
