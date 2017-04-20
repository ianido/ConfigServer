using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using yupisoft.ConfigServer.Core.Utils;

namespace yupisoft.ConfigServer.Core.Services
{
    public class HttpServiceCheck : ServiceCheck
    {

        private object _lock = new object();
        private JServiceConfig _serviceConfig;

        public HttpServiceCheck(JServiceCheckConfig checkConfig, JServiceConfig serviceConfig, ILogger logger) : base(checkConfig, logger)
        {
            _serviceConfig = serviceConfig;
        }

        protected override void CheckAsync()
        {
            string http = _checkConfig.Http;
            http = http.Replace("$address", _serviceConfig.Address);
            http = http.Replace("$port", _serviceConfig.Port.ToString());
            HttpClient client = new HttpClient();
            client.Timeout = Timeout;
            client.GetAsync(http).ContinueWith((a) =>
            {
                lock (_lock)
                {
                    if ((a.Status == TaskStatus.RanToCompletion) && (a.IsCompleted) && (a.Result.IsSuccessStatusCode))
                        _lastCheckStatus = ServiceCheckStatus.Passing;
                    else
                        _lastCheckStatus = ServiceCheckStatus.Failing;
                    client.Dispose();
                    OnCheckDone(Id, _lastCheckStatus);
                }
            });
        }
    }
}
