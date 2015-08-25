using Microsoft.Framework.Configuration;
using nsteam.ConfigServer.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nsteam.ConfigServer.Provider
{
    public class CfgConfigurationSource : ConfigurationSource
    {
        public string ServiceUrl { get; }

        public CfgConfigurationSource() : this(null){}

        public CfgConfigurationSource(string serviceurl)
        {
            ServiceUrl = serviceurl;
        }

        public override void Load()
        {
            ConfigService cfg = null;

            if (string.IsNullOrEmpty(ServiceUrl))
                cfg = new ConfigService();                
            else
                cfg = new ConfigService(ServiceUrl);

            dynamic obj = cfg.GetTree();
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            Data["CfgServerSettings"] = json;
            //JsonConfigurationFileParser parser = new JsonConfigurationFileParser();
            //Data = parser.Parse(json);
        }

    }
}
