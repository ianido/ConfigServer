using ARSoft.Tools.Net.Dns;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using yupisoft.ConfigServer.Core.Services;

namespace yupisoft.ConfigServer.Core
{
    public class ConfigServerHooks 
    {
        private static object _lock = new object();
        private ILogger _logger;
        private Timer _timer;
        private ConfigServerTenants _tenants;
        private bool _Monitoring = false;

        public ConfigServerHooks(ILogger<ConfigServerHooks> logger, ConfigServerTenants tenants)
        {
            _logger = logger;
            _tenants = tenants;
            _timer = new Timer(new TimerCallback(Timer_Elapsed), tenants, Timeout.Infinite, 1000);
            _logger.LogInformation("Created ConfigServerHooks with " + tenants.Tenants.Count + " tenants.");
        }

        public void StopMonitoring()
        {
            lock (_lock)
            {
                _Monitoring = false;
                _timer.Change(Timeout.Infinite, 1000);
            }
        }

        public void StartMonitoring()
        {
            lock (_lock)
            {
                _Monitoring = true;
                _timer.Change(1000, 1000);
            }
        }
        
        private void Timer_Elapsed(object state)
        {
            lock (_lock)
            {
                if (_Monitoring)
                {
                    foreach (var tenant in _tenants.Tenants)
                    {
                        foreach (var hook in tenant.Hooks)
                        {
                            hook.Value.Check();
                        }
                    }
                }
            }
        }

    }
}
