using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using yupisoft.ConfigServer.Core.Services;

namespace yupisoft.ConfigServer.Core
{
    public class ConfigServerServices 
    {
        private static object objlock = new object();
        private ILogger _logger;
        private Timer _timer;
        private ConfigServerTenants _tenants;

        public ConfigServerServices(ILogger<ConfigServerServices> logger, ConfigServerTenants tenants)
        {
            _logger = logger;
            _tenants = tenants;
            _timer = new Timer(new TimerCallback(Timer_Elapsed), tenants, Timeout.Infinite, 1000);

            foreach (var tenant in _tenants.Tenants)
            {
                foreach (var service in tenant.Services)
                {
                    service.Value.AllChecksDone += Services_AllChecksDone;
                }
            }

            _logger.LogInformation("Created ConfigServerServices with " + tenants.Tenants.Count + " tenants.");
        }

        private void Services_AllChecksDone(Service service, Services.ServiceCheckStatus status)
        {
            _logger.LogInformation("Service: " + service.Id + " check with status: " + status.ToString());
        }

        private void DisableTimer()
        {
            _timer.Change(Timeout.Infinite, 1000);
        }

        private void EnableTimer()
        {
            _timer.Change(1000, 1000);
        }

        private void Timer_Elapsed(object state)
        {
            foreach(var tenant in _tenants.Tenants)
            {
                foreach (var service in tenant.Services)
                {
                    int callid = service.Value.Check();
                }
            }
        }

    }
}
