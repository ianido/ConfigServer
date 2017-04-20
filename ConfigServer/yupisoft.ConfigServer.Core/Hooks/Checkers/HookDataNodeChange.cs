using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using yupisoft.ConfigServer.Core.Utils;

namespace yupisoft.ConfigServer.Core.Hooks
{
    public class HookDataNodeChange : Hook
    {
        /// <summary>
        /// Node to check for changes
        /// </summary>
        public string Node { get { return Config.Node; } }

        private ulong _lastHashCode = 0;
        protected ConfigServerTenant _Tenant { get; private set; }

        public HookDataNodeChange(JHookConfig config, ILogger logger, ConfigServerTenant tenant) : base(config, logger){
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
            //TODO: Hay que probar modificar, remover y agregar el nodo para ver si manda las notificaciones en todos los casos.

            JToken token = _Tenant.Token.SelectToken(this.Node);
            IHookCheckResult result = new HookCheckResult();
            result.HookId = Id;

            if (token == null)
            {                
                if (_lastHashCode > 1)
                {
                    result.Result = HookCheckStatus.DeleteItem;
                    _lastHashCode = 1;
                    Notify(result);
                    return result;
                } else
                {
                    _logger.LogWarning("Hook(" + this.Id + "): token not found.");
                    _lastHashCode = 1;
                    return result;
                }
            }

            string stringToken = token.ToString(Formatting.None);
            var hashCode = StringHandling.CalculateHash(stringToken);
            
            if (_lastHashCode == 0) {
                _lastHashCode = hashCode;
                return result;
            }
            else
            if (_lastHashCode != hashCode)
            {
                result.Data = token;

                if (_lastHashCode == 1)
                    result.Result = HookCheckStatus.AddedItem;
                else
                    result.Result = HookCheckStatus.ChangeItem;

                _lastHashCode = hashCode;

                Notify(result);
                return result;                
            }
            return result;
        } 
    }
}
