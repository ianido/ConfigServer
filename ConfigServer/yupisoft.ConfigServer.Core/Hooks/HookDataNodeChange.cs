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
        public string Node { get { return Config.Node; } }
        public string Http { get { return Config.Http; } }
        public string HttpMethod { get { return Config.HttpMethod; } }

        private ulong _lastHashCode = 0;

        public HookDataNodeChange(JHookConfig config, ILogger logger, ConfigServerTenant tenant) : base(config, logger, tenant)
        {
        }

        private async void Notify(JObject data)
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage request = null;

            if (HttpMethod.ToLower() == "post") request = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, Http);
            if (HttpMethod.ToLower() == "get") request = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, Http);
            if (HttpMethod.ToLower() == "put") request = new HttpRequestMessage(System.Net.Http.HttpMethod.Put, Http);
            if (HttpMethod.ToLower() == "delete") request = new HttpRequestMessage(System.Net.Http.HttpMethod.Delete, Http);

            request.Content = new StringContent(data.ToString(Formatting.None), Encoding.UTF8, "application/json");
            _logger.LogTrace("Hook(" + Id + ") Invoked.");

            await client.SendAsync(request).ContinueWith((a) =>
            {
                if ((a.Status == TaskStatus.RanToCompletion) && (a.Result.IsSuccessStatusCode))
                    _logger.LogTrace("Hook(" + Id + ") StatusCode: " + a.Result.StatusCode);
                else
                    _logger.LogTrace("Hook(" + Id + ") Task Failed: " + a.Status);
                OnCheckDone(HookCheckStatus.Notify);
            });
        }

        protected override async Task<HookCheckStatus> CheckAsync(){
            //TODO: Hay que probar modificar, remover y agregar el nodo para ver si manda las notificaciones en todos los casos.

            JToken token = _Tenant.Token.SelectToken(this.Node);
            JObject data = JObject.Parse("{'result':'iddle','node':''}");

            if (token == null)
            {                
                if (_lastHashCode > 1)
                {
                    data["result"] = "removed";
                    _lastHashCode = 1;
                    Notify(data);
                    return HookCheckStatus.Notify;
                } else
                {
                    _logger.LogWarning("Hook(" + this.Id + "): token not found.");
                    _lastHashCode = 1;
                    return HookCheckStatus.Iddle;
                }
            }

            string stringToken = token.ToString(Formatting.None);
            var hashCode = StringHandling.CalculateHash(stringToken);
            
            if (_lastHashCode == 0) {
                _lastHashCode = hashCode;
                return HookCheckStatus.Iddle;
            }
            else
            if (_lastHashCode != hashCode)
            {
                data["data"] = token;

                if (_lastHashCode == 1)
                    data["result"] = "added";
                else
                    data["result"] = "modified";

                _lastHashCode = hashCode;

                if (!string.IsNullOrEmpty(Http))
                {
                    Notify(data);
                    return HookCheckStatus.Notify;
                }
                else {
                    _logger.LogError("Hook(" + this.Id + "): Http address not valid.");
                    return HookCheckStatus.Iddle;
                }
            }
            return HookCheckStatus.Iddle;
        } 
    }
}
