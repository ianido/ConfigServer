using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace nsteam.ConfigServer.Types
{
    public class ConfigEntity
    {
        public string FileName { get; set; }
        public string GroupName { get; set; }
        public XmlDocument Root { get; set; }
        public XmlDocument RootReferences { get; set; }
    }
}
