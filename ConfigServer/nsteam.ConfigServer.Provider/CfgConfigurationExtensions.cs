using System;
using System.IO;
using Microsoft.Framework.Configuration.Helper;
using Microsoft.Framework.Internal;
using Microsoft.Framework.Configuration;

namespace nsteam.ConfigServer.Provider
{
    public static class CfgConfigurationExtensions
    {
        public static IConfigurationBuilder AddConfigServer(this IConfigurationBuilder configuration)
        {
            return AddConfigServer(configuration, null);
        }

        public static IConfigurationBuilder AddConfigServer(this IConfigurationBuilder configuration, string url)
        {
            if (string.IsNullOrEmpty(url))
                configuration.Add(new CfgConfigurationSource());
            else
                configuration.Add(new CfgConfigurationSource(url));

            return configuration;
        }

        public static IConfigurationBuilder AddConfigServer(this IConfigurationBuilder configuration, string url, string basenode)
        {
            if (string.IsNullOrEmpty(url))
                configuration.Add(new CfgConfigurationSource());
            else
                configuration.Add(new CfgConfigurationSource(url, basenode));

            return configuration;
        }
    }
}
