using NLog.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nsteam.ConfigServer.Types
{
    public class AppConfigService : IConfigurationService
    {
        public dynamic GetSetting(string settingName)
        {
            return new ConfigurationManager().AppSettings[settingName];
        }

        public T GetSetting<T>(string settingName)
        {
            throw new NotSupportedException();
        }

        public void SetSetting(string settingName, dynamic settingValue)
        {
            new ConfigurationManager().AppSettings[settingName] = settingValue;
        }
    }
}
