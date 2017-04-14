using ARSoft.Tools.Net.Dns;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace yupisoft.ConfigServer.Core.Services
{
    public class DNSServer : IServiceDiscovery
    {
        private DNSConfigSection _config;
        private ConfigServerTenants _tenants;
        private ILogger _logger;
        private DnsServer _dnsServer;


        public DNSServer(DNSConfigSection config, ILogger logger, ConfigServerTenants tenants)
        {
            _config = config;
            _logger = logger;
            _tenants = tenants;
        }

        public void AttemptStart()
        {
            if (_config.Enabled)
            {
                try
                {
                    _logger.LogInformation("Starting DNS Server on Port:" + _config.Port + " Masterzone: " + _config.Masterzone + " Bound to Interface :" + _config.BindAddress);
                    IPAddress addr = IPAddress.Any;
                    if (_config.BindAddress.ToLower() != "any") addr = IPAddress.Parse(_config.BindAddress);
                    IPEndPoint endpoint = new IPEndPoint(addr, _config.Port);

                    _dnsServer = new DnsServer(endpoint, _config.udpListenerCount, _config.tcpListenerCount);
                    _dnsServer.ClientConnected += Server_ClientConnected;
                    _dnsServer.QueryReceived += Server_QueryReceived;

                    _dnsServer.Start();
                }
                catch (Exception ex)
                {
                    _logger.LogError("Unable to start DNS Server: " + ex.Message);

                }
            }
            else
                _logger.LogInformation("DNS Server is disabled");
        }

        public void AttemptStop()
        {
            if (_dnsServer != null) _dnsServer.Stop();            
        }


        private Task Server_QueryReceived(object sender, QueryReceivedEventArgs eventArgs)
        {
            foreach (var tenant in _tenants.Tenants)
            {
                if (tenant.EnableServiceDiscovery)
                {
                    foreach (var service in tenant.Services)
                    {

                    }
                }
            }
            throw new NotImplementedException();
        }

        private Task Server_ClientConnected(object sender, ClientConnectedEventArgs eventArgs)
        {
            throw new NotImplementedException();
        }
    }
}
