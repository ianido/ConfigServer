using Microsoft.Framework.Configuration;
using nsteam.ConfigServer.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nsteam.ConfigServer.Provider
{
    public class CfgConfigurationSource : ConfigurationProvider
    {
        public string ServiceUrl { get; }
        public string BaseNode { get; }

        public CfgConfigurationSource() : this(null){}

        public CfgConfigurationSource(string serviceurl)
        {
            ServiceUrl = serviceurl;
        }

        public CfgConfigurationSource(string serviceurl, string basenode)
        {
            ServiceUrl = serviceurl;
            BaseNode = basenode;
        }

        public override void Load()
        {
            ConfigService cfg = null;

            if (string.IsNullOrEmpty(ServiceUrl))
                cfg = new ConfigService();                
            else
                if (string.IsNullOrEmpty(BaseNode))
                    cfg = new ConfigService(ServiceUrl);
                else
                    cfg = new ConfigService(ServiceUrl, BaseNode);

            dynamic obj = cfg.GetTree();
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            JsonConfigurationFileParser parser = new JsonConfigurationFileParser();
            Data = parser.Parse("{ CfgServer:" + json + " }");            
        }

    }
}
