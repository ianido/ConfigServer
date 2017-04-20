using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using yupisoft.ConfigServer.Core.Services;
using yupisoft.ConfigServer.Core.Utils;

namespace yupisoft.ConfigServer.Core.Hooks
{
    public class HookServiceStatusChange : Hook
    {
        /// <summary>
        /// Service Node to monitor State
        /// </summary>
        public string ServiceName { get { return Config.ServiceName; } }
        public string NotifyOn { get { return Config.NotifyOn; } }

        private ServiceCheckStatus _lastStatus = ServiceCheckStatus.Iddle;
        protected ConfigServerTenant _Tenant { get; private set; }

        public HookServiceStatusChange(JHookConfig config, ILogger logger, ConfigServerTenant tenant) : base(config, logger){
            _Tenant = tenant;
        }

        private void Notify(IHookCheckResult result)
        {
            foreach (var nots in Notifications)
            {
                nots.Notify(result);
            }
        }

        protected override async Task<IHookCheckResult> CheckAsync(){
            IHookCheckResult result = new HookCheckResult();
            result.HookId = Config.Id;
            result.Result = HookCheckStatus.Iddle;
            foreach (var service in _Tenant.Services)
            {
                if (service.Value.Name == ServiceName)
                {
                    var srvStatus = service.Value.LastCheckStatus;
                    if (_lastStatus == ServiceCheckStatus.Failing)
                        _lastStatus = srvStatus;
                    else
                    if (_lastStatus != srvStatus)
                    {
                        string[] notifiers = NotifyOn.ToLower().Split(',');
                        if ((notifiers.Length > 0) && (notifiers.Contains(srvStatus.ToString().ToLower())))
                        {
                            result.Result = HookCheckStatus.ChangeStatus;
                            result.Data = JObject.Parse("{ 'statusfrom':'" + _lastStatus.ToString().ToLower() + "','statusto':'" + srvStatus.ToString().ToLower() + "'  }");
                            Notify(result);
                        }
                    }
                }
            }
            return result;
        } 
    }
}
