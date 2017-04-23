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
        public string ServiceId { get { return Config.ServiceId; } }
        public string NotifyOn { get { return Config.NotifyOn??""; } }

        private DateTime _lastStatusDate = DateTime.UtcNow;
        private ServiceCheckStatus _lastStatus = ServiceCheckStatus.Iddle;
        protected ConfigServerTenant _Tenant { get; private set; }

        public HookServiceStatusChange(JHookConfig config, ILogger logger, ConfigServerTenant tenant) : base(config, logger){
            _Tenant = tenant;
            if (!string.IsNullOrEmpty(config.ServiceId) && !string.IsNullOrEmpty(config.ServiceName))
                throw new Exception("Hook: You have to Choose ServiceId or ServiceName, but not both.");
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
            result.Hook = this;
            result.Result = HookCheckStatus.Iddle;

            var currentServiceStatus = ServiceCheckStatus.Iddle;

            if (!string.IsNullOrEmpty(ServiceName))
            {
                var srvNameLastStatus = ServiceCheckStatus.Iddle;
                var groups = _Tenant.Services.GroupBy(g => g.Value.Name);
                foreach (var g in groups)
                {
                    if (g.Key == ServiceName)
                    {
                        //_logger.LogTrace("Hook(" + Id + ") Service Name: " + ServiceName + " Checking.");
                        foreach (var service in g)
                        {
                            if (service.Value.LastCheckStatus > ServiceCheckStatus.InProgress 
                                && (service.Value.LastCheckStatus > srvNameLastStatus))
                                srvNameLastStatus = service.Value.LastCheckStatus;
                        }
                        currentServiceStatus = srvNameLastStatus;
                        break;
                    }
                }
            }
            else if (!string.IsNullOrEmpty(ServiceId))
            {
                foreach (var service in _Tenant.Services)
                {
                    if (service.Value.Id == ServiceId)
                    {
                        if (service.Value.LastCheckStatus > ServiceCheckStatus.InProgress)
                            currentServiceStatus = service.Value.LastCheckStatus;
                        break;                        
                    }
                }
            }

            if ((_lastStatus == ServiceCheckStatus.Iddle) || (currentServiceStatus == ServiceCheckStatus.Iddle))
            {
                _lastStatus = currentServiceStatus;
                _lastStatusDate = DateTime.UtcNow;
            }
            else
            if (_lastStatus != currentServiceStatus)
            {
                string[] notifiers = NotifyOn.ToLower().Split(',');
                result.Result = HookCheckStatus.ChangeStatus;
                var currentServiceStatusUpdate = DateTime.UtcNow;
                result.Data = JObject.Parse("{ 'servicename':'" + ServiceName + "'," +
                                              "'serviceid':'" + ServiceId + "'," +
                                              "'description':'" + Description + "'," +
                                              "'statusfrom':'" + _lastStatus.ToString().ToLower() + "'," +
                                              "'statusto':'" + currentServiceStatus.ToString().ToLower() + "',"+
                                              "'timespan':'" + (currentServiceStatusUpdate - _lastStatusDate).Human() + "'," +
                                              "'lastupdate':'" + currentServiceStatusUpdate.ToString() + "',"+
                                              "'prevupdate':'" + _lastStatusDate.ToString() + "'}");
                _lastStatus = currentServiceStatus;
                _lastStatusDate = currentServiceStatusUpdate;
                if ((notifiers.Length > 0) && (notifiers[0] == "" || (notifiers.Contains(currentServiceStatus.ToString().ToLower()))))
                    Notify(result);                    
                return result;
            }
            return result;
        } 
    }
}
