using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;

namespace nsteam.ConfigServer.Client
{

    public class ClientConfigSection : ConfigurationSection
    {
        public static ClientConfigSection GetSettings()
        {
            return (ClientConfigSection)System.Configuration.ConfigurationManager.GetSection("ConfigServer");
        }

        [ConfigurationProperty("Server", DefaultValue = "", IsRequired = true)]
        public string Server
        {
            get
            {
                return (string)this["Server"];
            }
            set
            {
                this["Server"] = value;
            }
        }

        [ConfigurationProperty("BaseNode", DefaultValue = "", IsRequired = false)]
        public string BaseNode
        {
            get
            {
                return (string)this["BaseNode"];
            }
            set
            {
                this["BaseNode"] = value;
            }
        }

        
    }
}
