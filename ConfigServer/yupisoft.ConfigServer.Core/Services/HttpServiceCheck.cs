using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace yupisoft.ConfigServer.Core.Services
{
    public class HttpServiceCheck : ServiceCheck
    {
        private JServiceCheckConfig _checkConfig;
        
        private object _locking;

        public HttpServiceCheck(JServiceCheckConfig checkConfig) : base(checkConfig)
        {
            _checkConfig = checkConfig;
        }

        public override void Check()
        {
            HttpClient client = new HttpClient();
            client.Timeout = _checkConfig.TimeoutSpan;
            client.GetAsync(_checkConfig.Http).ContinueWith((a) =>
            {
                lock (_locking)
                {
                    if (a.Result.IsSuccessStatusCode)
                        _lastCheckStatus = ServiceCheckStatus.Passing;
                    else
                        _lastCheckStatus = ServiceCheckStatus.Failing;
                    client.Dispose();
                }
            });
        }
    }
}
