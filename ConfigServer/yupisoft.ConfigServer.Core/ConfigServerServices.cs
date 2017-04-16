﻿using ARSoft.Tools.Net.Dns;
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
    public class ConfigServerServices 
    {
        private static object _lock = new object();
        private ILogger _logger;
        private Timer _timer;
        private ConfigServerTenants _tenants;
        private ServiceDiscoveryConfigSection _sdConfig;
        private IServiceDiscovery[] servers;
        private bool _Monitoring = false;

        public ConfigServerServices(IOptions<ServiceDiscoveryConfigSection> sdConfig, ILogger<ConfigServerServices> logger, ConfigServerTenants tenants)
        {
            _logger = logger;
            _tenants = tenants;
            _sdConfig = sdConfig.Value;
            _timer = new Timer(new TimerCallback(Timer_Elapsed), tenants, Timeout.Infinite, 1000);
            _logger.LogInformation("Created ConfigServerServices with " + tenants.Tenants.Count + " tenants.");

            //Create Service Discovery Engines
            List<IServiceDiscovery> serv = new List<IServiceDiscovery>();
            serv.Add(new DNSServer(_sdConfig.DNS, _logger, _tenants));
            servers = serv.ToArray();
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

        public void StartServiceDiscovery()
        {
            foreach(var sd in servers)
            {
                sd.AttemptStart();
            }
        }

        public void StopServiceDiscovery()
        {
            foreach (var sd in servers)
            {
                sd.AttemptStop();
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
                        foreach (var service in tenant.Services)
                        {
                            service.Value.Check();
                        }
                    }
                }
            }
        }

    }
}
