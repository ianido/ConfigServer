using System;
using System.Collections.Generic;
using System.Text;

namespace yupisoft.ConfigServer.Core.Services
{
    public enum GeoIPResponseStatus
    {
        Success,
        Failed,
        Exception
    }

    public class GeoLocation
    {
        public string Country { get; set; }
        public string CountryCode { get; set; }
        public string Region { get; set; }
        public string RegionCode { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
        public string Lat { get; set; }
        public string Lon { get; set; }
        public string Timezone { get; set; }
        public string Isp { get; set; }
        public string Organization { get; set; }
    }
    
    public class GeoIPResponse : GeoLocation
    {
        public GeoIPResponseStatus Status { get; set; }
        public string ErrorMessage { get; set; }
    }

}
