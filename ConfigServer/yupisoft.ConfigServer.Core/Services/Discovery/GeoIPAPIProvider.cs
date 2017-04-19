using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace yupisoft.ConfigServer.Core.Services
{
    public class GeoIPAPIProvider : IGeoIPServiceProvider
    {
        public GeoIPResponse GeoLocate(string ipAddress)
        {
            HttpClient client = new HttpClient();
            GeoIPResponse response = new GeoIPResponse();
            try
            {
                var result = client.PostAsync("http://ip-api.com/json/" + ipAddress, new StringContent("", Encoding.UTF8, "application/json")).Result;
                JObject obj = JsonConvert.DeserializeObject<JObject>(result.Content.ReadAsStringAsync().Result);
                if (obj["status"].Value<string>() == "success")
                {
                    response.Status = GeoIPResponseStatus.Success;
                    response.Country = obj["country"].Value<string>();
                    response.CountryCode = obj["countryCode"].Value<string>();
                    response.Region = obj["regionName"].Value<string>();
                    response.RegionCode = obj["region"].Value<string>();
                    response.City = obj["city"].Value<string>();
                    response.Isp = obj["isp"].Value<string>();
                    response.Lat = obj["lat"].Value<string>();
                    response.Lon = obj["lon"].Value<string>();
                    response.Organization = obj["org"].Value<string>();
                    response.Timezone = obj["timezone"].Value<string>();
                    response.Zip = obj["zip"].Value<string>();
                } else
                {
                    response.Status = GeoIPResponseStatus.Failed;
                    response.ErrorMessage = obj["message"].Value<string>();
                }
            }
            catch(Exception ex)
            {
                response.Status = GeoIPResponseStatus.Exception;
                response.ErrorMessage = ex.Message;
            }
            return response;

        }
    }
}
