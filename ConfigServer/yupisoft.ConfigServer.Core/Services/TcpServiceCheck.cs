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

        public TcpServiceCheck(JServiceCheckConfig checkConfig, JServiceConfig serviceConfig) : base(checkConfig, serviceConfig)
        {
            _checkConfig = checkConfig;
            _checkConfig.Tcp = _checkConfig.Tcp.Replace("$address", serviceConfig.Address);
            _checkConfig.Tcp = _checkConfig.Tcp.Replace("$port", serviceConfig.Port.ToString());
        }


        protected override void CheckAsync(int callid)
        {
            TcpClient client = new TcpClient();
            var addr = _checkConfig.Tcp.Split(':');
            if (addr.Length < 2) throw new Exception("Tcp address needs a port.");
            client.ConnectAsync(IPAddress.Parse(addr[0]), int.Parse(addr[1])).ContinueWith((a) =>
            {
                lock (_lock)
                {
                    if ((a.Status == TaskStatus.RanToCompletion) && (a.IsCompleted) && (client.Connected))
                        _lastCheckStatus = ServiceCheckStatus.Passing;                       
                    else
                        _lastCheckStatus = ServiceCheckStatus.Failing;
                    client?.Dispose();
                    OnCheckDone(Id, _lastCheckStatus, callid);
                }
            }).WithTimeout(Timeout);
        }
    }
}
