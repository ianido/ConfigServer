using System;
using System.Collections.Generic;
using System.Text;

namespace yupisoft.ConfigServer.Core.Services
{



    public interface IGeoIPServiceProvider
    {
        GeoIPResponse GeoLocate(string ipAddress);
    }
}
