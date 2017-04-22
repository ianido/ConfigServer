using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace yupisoft.ConfigServer.Core.Services
{
    public class TcpServiceCheck : ServiceCheck
    {
        
        private object _lock = new object();
        private JServiceConfig _serviceConfig;

        public TcpServiceCheck(JServiceCheckConfig checkConfig, JServiceConfig serviceConfig, ILogger logger) : base(checkConfig, logger)
        {
            _serviceConfig = serviceConfig;
        }


        protected override void CheckAsync()
        {
            string tcp = _checkConfig.Tcp;
            tcp = tcp.Replace("$address", _serviceConfig.Address);
            tcp = tcp.Replace("$port", _serviceConfig.Port.ToString());

            TcpClient client = new TcpClient();
            var addr = tcp.Split(':');
            if (addr.Length < 2)
            {
                _logger.LogWarning("Cant perform TCP Check("+ Id +") - TCP Address needs a port.");
                return;
            }
            client.ConnectAsync(addr[0], int.Parse(addr[1])).ContinueWith((a) =>
            {
                lock (_lock)
                {
                    if ((a.Status == TaskStatus.RanToCompletion) && (a.IsCompleted) && (client.Connected))
                        _lastCheckStatus = ServiceCheckStatus.Passing;                       
                    else
                        _lastCheckStatus = ServiceCheckStatus.Failing;
                    client?.Dispose();
                    OnCheckDone(Id, _lastCheckStatus);
                }
            }).WithTimeout(Timeout);
        }
    }
}
