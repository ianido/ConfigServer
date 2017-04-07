using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using yupisoft.ConfigServer.Core.Utils;

namespace yupisoft.ConfigServer.Core.Services
{
    public class HttpServiceCheck : ServiceCheck
    {

        private object _locking = new object();

        public HttpServiceCheck(JServiceCheckConfig checkConfig) : base(checkConfig)
        {
            _checkConfig = checkConfig;
        }

        public override void Check(int callid)
        {
            HttpClient client = new HttpClient();
            client.Timeout = Timeout;
            client.GetAsync(_checkConfig.Http).ContinueWith((a) =>
            {
                lock (_locking)
                {
                    if (a.Result.IsSuccessStatusCode)
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
