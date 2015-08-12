using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace nsteam.ConfigServer.Types
{

    public class ConfigNode
    {
        public int Id { get; set; }
        public string CodeName { get; set; }
        public string Path { get; set; }
        public dynamic Ext { get; set; }
        public List<ConfigNode> Children { get; set; }

        public ConfigNode()
        {
            Children = new List<ConfigNode>();            
        }

        
    }

}
