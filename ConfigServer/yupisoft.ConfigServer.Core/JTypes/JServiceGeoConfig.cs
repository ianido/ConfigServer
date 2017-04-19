using Newtonsoft.Json.Linq;
using System.Dynamic;

namespace yupisoft.ConfigServer.Core
{
    public class JServiceGeoConfig
    {
        public string Continents { get; set; }
        public string Countries { get; set; }
        public string Regions { get; set; }
        public string GeoPos { get; set; }

        public JServiceGeoConfig()
        {
            Continents = "";
            Countries = "";
            Regions = "";
            GeoPos = "";
        }
    }
}
