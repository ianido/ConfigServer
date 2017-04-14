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

        public HttpServiceCheck(JServiceCheckConfig checkConfig, JServiceConfig serviceConfig) : base(checkConfig, serviceConfig)
        {
            _checkConfig = checkConfig;
            _checkConfig.Http = _checkConfig.Http.Replace("$address", serviceConfig.Address);
            _checkConfig.Http = _checkConfig.Http.Replace("$port", serviceConfig.Port.ToString());
        }

        protected override void CheckAsync(int callid)
        {
            HttpClient client = new HttpClient();
            client.Timeout = Timeout;
            client.GetAsync(_checkConfig.Http).ContinueWith((a) =>
            {
                lock (_lock)
                {
                    if ((a.Status == TaskStatus.RanToCompletion) && (a.IsCompleted) && (a.Result.IsSuccessStatusCode))
                        _lastCheckStatus = ServiceCheckStatus.Passing;
                    else
                        _lastCheckStatus = ServiceCheckStatus.Failing;
                    client.Dispose();
                    OnCheckDone(Id, _lastCheckStatus, callid);
                }
            });
        }
    }
}
