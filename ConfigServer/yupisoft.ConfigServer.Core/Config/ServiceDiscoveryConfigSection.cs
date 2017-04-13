using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace yupisoft.ConfigServer.Core
{

    public class DNSConfigSection
    {
        public int Port { get; set; }
        public bool Enabled { get; set; }
        public bool FordwardingEnabled { get; set; }        
        public string BindAddress { get; set; }
        public string Masterzone { get; set; }
        public int udpListenerCount { get; set; }
        public int tcpListenerCount { get; set; }

        public DNSConfigSection()
        {
            udpListenerCount = 10;
            tcpListenerCount = 10;
            Masterzone = "configserver";
            Port = 53;
            BindAddress = "Any";
            FordwardingEnabled = true;
        }
    }

    public class ServiceDiscoveryConfigSection
    {
        public DNSConfigSection DNS { get; set; }
        public ServiceDiscoveryConfigSection()
        {
            DNS = new DNSConfigSection();
        } 
    }
}
