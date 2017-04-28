using ARSoft.Tools.Net.Dns;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using yupisoft.ConfigServer.Core.Cluster;
using yupisoft.ConfigServer.Core.Services;

namespace yupisoft.ConfigServer.Core
{
    public class ConfigServerServices 
    {
        private static object _lock = new object();
        private ILogger _logger;
        private Timer _timer;
        private ConfigServerTenants _tenants;
        private IClusterManager _clusterMan;
        private ServiceDiscoveryConfigSection _sdConfig;
        private GeoServices _geoServices;
        private IServiceDiscovery[] servers;
        private bool _Monitoring = false;

        public ConfigServerServices(IOptions<ServiceDiscoveryConfigSection> sdConfig, ILogger<ConfigServerServices> logger, ConfigServerTenants tenants, IClusterManager clusterMan, GeoServices geoServices)
        {
            _logger = logger;
            _tenants = tenants;
            _clusterMan = clusterMan;
            _geoServices = geoServices;
            _sdConfig = sdConfig.Value;
            _timer = new Timer(new TimerCallback(Timer_Elapsed), tenants, Timeout.Infinite, _sdConfig.CheckingInterval);
            if (!_sdConfig.Enabled)
                _logger.LogInformation("ServiceDiscovery: NOT enabled in this Node.");
            else
                _logger.LogInformation("ServiceDiscovery: ENABLED with " + tenants.Tenants.Count + " tenants.");

            //Create Service Discovery Engines
            List<IServiceDiscovery> serv = new List<IServiceDiscovery>();
            if (_sdConfig.Enabled && _sdConfig.DNS.Enabled)
                serv.Add(new DNSServer(_sdConfig.DNS, _logger, _tenants, _clusterMan, _geoServices));
            servers = serv.ToArray();
        }

        public void StopMonitoring()
        {
            lock (_lock)
            {
                if (!_sdConfig.Enabled) { return; }
                _Monitoring = false;
                _timer.Change(Timeout.Infinite, _sdConfig.CheckingInterval);
            }
        }

        public void StartMonitoring()
        {
            lock (_lock)
            {
                if (!_sdConfig.Enabled) { return; }
                _Monitoring = true;
                _timer.Change(_sdConfig.CheckingInterval, _sdConfig.CheckingInterval);
            }
        }

        public void StartServiceDiscovery()
        {
            if (!_sdConfig.Enabled) { return; }
            foreach (var sd in servers)
            {
                sd.AttemptStart();
            }
        }

        public void StopServiceDiscovery()
        {
            if (!_sdConfig.Enabled) {  return; }
            foreach (var sd in servers)
            {
                sd.AttemptStop();
            }
        }

        private void Timer_Elapsed(object state)
        {
            lock (_lock)
            {
                if (!_sdConfig.Enabled) { return; }
                if (_Monitoring)
                {
                    foreach (var tenant in _tenants.Tenants)
                    {
                        try
                        {
                            foreach (var service in tenant.Services)
                            {
                                service.Value.Check();
                            }
                        }
                        catch(Exception ex)
                        {
                            _logger.LogError("Exception running Checks for tenant: " + tenant.Id + ". Message: " +ex.ToString());
                        }
                    }
                }
            }
        }


    }
}
