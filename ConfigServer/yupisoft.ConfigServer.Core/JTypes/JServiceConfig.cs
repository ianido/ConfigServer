using Newtonsoft.Json.Linq;
using System.Dynamic;

namespace yupisoft.ConfigServer.Core
{
    public class JServiceConfig
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }
        public string[] Tags { get; set; }
        public dynamic Config { get; set; }
        public JServiceCheckConfig[] Checks { get; set; }

        public JServiceConfig()
        {
            Checks = new JServiceCheckConfig[0];
            Tags = new string[0];
        }

    }
}
