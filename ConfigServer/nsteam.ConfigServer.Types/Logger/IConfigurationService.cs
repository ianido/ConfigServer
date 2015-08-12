using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace nsteam.ConfigServer.Types
{
    public interface IConfigurationService
    {
        dynamic GetSetting(string settingName);
        T GetSetting<T>(string settingName);
        void SetSetting(string settingName, dynamic settingValue);
    }
}
