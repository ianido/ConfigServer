using Newtonsoft.Json.Linq;
using System.Dynamic;

namespace yupisoft.ConfigServer.Core
{
    public class JServiceGeoConfig
    {
        public string Region { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        
        public JServiceGeoConfig()
        {
            Region = "*";
            Country = "*";
            State = "*";
        }
    }
}
