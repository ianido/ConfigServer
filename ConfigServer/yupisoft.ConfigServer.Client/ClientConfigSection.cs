using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
#if NET462
using System.Configuration;
#endif
using System.Linq;
using System.Text;

namespace yupisoft.ConfigServer.Client
{
#if NET462
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
        [ConfigurationProperty("TenantId", DefaultValue = "", IsRequired = false)]
        public string TenantId
        {
            get
            {
                return (string)this["TenantId"];
            }
            set
            {
                this["TenantId"] = value;
            }
        }  
        [ConfigurationProperty("APPId", DefaultValue = "", IsRequired = false)]
        public string APPId
        {
            get
            {
                return (string)this["APPId"];
            }
            set
            {
                this["APPId"] = value;
            }
        }  
        [ConfigurationProperty("APIKey", DefaultValue = "", IsRequired = false)]
        public string APIKey
        {
            get
            {
                return (string)this["APIKey"];
            }
            set
            {
                this["APIKey"] = value;
            }
        }  
    }
#endif
}
