using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using yupisoft.ConfigServer.Core.Utils;


namespace yupisoft.ConfigServer.Core.Cluster
{

    public class SoftClusterManager : ClusterManager
    {

        public SoftClusterManager (IOptions<DatacenterConfigSection> datacenterConfig, IOptions<ClusterConfigSection> clusterConfig, ILogger<IClusterManager> logger, ConfigurationChanger cfgChanger, ConfigServerTenants tenantManager) :
            base(datacenterConfig, clusterConfig, logger, cfgChanger, tenantManager)
        {

        }

        protected override void Timer_Elapsed(object state)
        {
            _heartbeatTimer.Change(Timeout.Infinite, _NodesMonitoringHeartbeat); // Disable the timer;
            if (selfNode.Mode == Node.NodeMode.Server)
            {
                Node[] nodes = null;
                lock (_nodes) { nodes = _nodes.ToArray(); }
                foreach (var w in nodes)
                {
                    lock (w)
                    {
                        if (!w.Active) continue;
                        if ((w.Self) || (w.InUse) || (w.Mode == Node.NodeMode.Client) || (!w.NodeConfig.HeartBeat)) continue;
                        if (w.DataCenter != DataCenterId) continue; // Only hearthbeat this DC nodes.
                    }
                    HeartBeatMessage req = null;
                    lock (selfNode)
                    {
                        req = CreateMessageFor(HeartBeartType.HeartBeatRequest, null);
                    }
                    SendHeartBeat(w, req);
                }
            }
            else
            {
                Node node = null;
                lock (_nodes)
                {
                    node = _nodes.OrderByDescending(n => n.Priority).FirstOrDefault(n => !n.Self && n.Active && n.Mode == Node.NodeMode.Server && n.NodeConfig.HeartBeat);
                }
                if (node != null)
                {
                    var req = CreateMessageFor(HeartBeartType.HeartBeatRequest, null);
                    SendHeartBeat(node, req);
                }
            }
            _heartbeatTimer.Change(rndGenerator.Next(_NodesMonitoringRndIntervalHeartbeat) + _NodesMonitoringHeartbeat, _NodesMonitoringHeartbeat); // Reenable the timer;
        }
        protected override void DataCenterTimer_Elapsed(object state)
        {
            _betweenDatacenterTimer.Change(Timeout.Infinite, _DataCenterMaxIntervalHeartBeat); // Disable the timer;
            bool sendHearthBeat = rndGenerator.Next(10) > 5;

            if (sendHearthBeat)
            {
                Node node = null;
                lock (_nodes)
                {
                    node = _nodes.OrderByDescending(n => n.Priority).FirstOrDefault(n => !n.Self && n.Active && n.Mode == Node.NodeMode.Server && n.NodeConfig.HeartBeat && n.NodeConfig.DataCenter != DataCenterId);
                }
                if (node != null)
                {
                    var req = CreateMessageFor(HeartBeartType.HeartBeatRequest, null);
                    SendHeartBeat(node, req);
                }
            }
            _betweenDatacenterTimer.Change(rndGenerator.Next(_DataCenterMaxIntervalHeartBeat) + _DataCenterMinIntervalHeartBeat, rndGenerator.Next(_DataCenterMaxIntervalHeartBeat) + _DataCenterMinIntervalHeartBeat);
        }
        protected override void Store_Change(ConfigServerTenant tenant, IStoreProvider sender, string entityName)
        {
            var loadResult = tenant.Load(false);
            if (loadResult.Changes.Length > 0)
                foreach (var e in loadResult.Changes)
                {
                    _logger.LogInformation("Data changed Tenant: " + tenant.Id + " Entity: " + e.entity);
                    _logger.LogTrace("Diff:" + e.diffToken?.ToString());

                    LogMessage lm = new LogMessage() { Created = DateTime.UtcNow, Entity = e.entity, TenantId = tenant.Id, JsonDiff = e.diffToken?.ToString(Formatting.None) };
                    lock (selfNode)
                    {
                        lm.LogId = (selfNode.LogMessages.Count > 0) ? (selfNode.LogMessages.Last().LogId + 1) : 1;
                        selfNode.LogMessages.Add(lm);
                    }
                }
        }
        private void UpdateNodeList(HeartBeatMessage message, HeartBeatMessage previousMsg)
        {
            #region message.Nodes --> Determine Discovery Node Collection Diferences
            // Check diferences, send only the diferences
            var currentNodes = _nodes.Select(n => n.NodeConfig).ToArray();
            var msgNodes = previousMsg.Nodes.Select(n => NodeConfigSection.Deserialize(n)).ToList();
            foreach (var node in msgNodes)
            {
                var mNode = currentNodes.FirstOrDefault(n => n.Id == node.Id);
                if (mNode == null)
                {
                    _nodes.Add(new Node(node));
                    _cfgChanger.AddClusterNode(node);
                }
            }

            foreach (var node in currentNodes)
            {
                var mNode = msgNodes.FirstOrDefault(n => n.Id == node.Id);
                if (mNode == null)
                    msgNodes.Add(node);
                else
                {
                    // Check for changes
                    if (mNode.Serialize() != node.Serialize())
                    {
                        if (mNode.Enabled && !node.Enabled)
                        {
                            node.CopyFrom(mNode);
                            _cfgChanger.UpdateClusterNode(node);
                        }
                        else
                        if (!mNode.Enabled && node.Enabled)
                            mNode.CopyFrom(node);
                        else
                        {
                            if (_AliveSince > previousMsg.NodeAliveSince)
                            {
                                node.CopyFrom(mNode);
                                _cfgChanger.UpdateClusterNode(node);
                            }
                            else
                                mNode.CopyFrom(node);
                        }
                    }
                }
            }
            message.Nodes = msgNodes.Select(n => n.Serialize()).ToArray();
            #endregion
        }
        private HeartBeatMessage CreateMessageFor(HeartBeartType type, HeartBeatMessage previousMsg)
        {
            HeartBeatMessage message = new HeartBeatMessage();
            message.Created = DateTime.UtcNow;
            message.NodeId = selfNode.Id;
            message.MessageType = type;
            message.NodeMode = selfNode.Mode;

            if (type == HeartBeartType.Unknow) return message;

            message.LastLogDate = selfNode.LastLogDate;
            message.LastLogId = selfNode.LastLogId;
            message.NodeAliveSince = _AliveSince;
            message.DataHash = _tenantManager.Tenants.Select(p => new TenantHash() { Id = p.TenantConfig.Id, Hash = p.DataHash }).ToArray();

            if ((type == HeartBeartType.HeartBeatRequest) )
            {
                message.Nodes = _nodes.Select(n => n.NodeConfig.Serialize()).ToArray();
            }
            else
            if ((type == HeartBeartType.HeartBeatResponse) || (type == HeartBeartType.HeartBeatUpdateResponse) || (type == HeartBeartType.UpdateResponse))
            {
                UpdateNodeList(message, previousMsg);
            }
            else
            if ((type == HeartBeartType.UpdateRequest))
            {
                UpdateNodeList(message, previousMsg);

                bool DifferentDataHash = false;
                foreach (var dh in message.DataHash)
                    if (previousMsg.DataHash.FirstOrDefault(e => e.Id == dh.Id).Hash != dh.Hash)
                    {
                        DifferentDataHash = true;
                        break;
                    }
                
                if (previousMsg.LastLogId < selfNode.LastLogId)
                {
                    var logMessages = selfNode.LogMessages.Where(m => m.LogId > previousMsg.LastLogId);
                    message.Log.AddRange(logMessages);
                    message.LastLogId = selfNode.LastLogId;
                    message.LastLogDate = selfNode.LastLogDate;
                }
                else
                    if ( DifferentDataHash &&
                           ( 
                            (selfNode.LastLogId == previousMsg.LastLogId) 
                             && ((message.NodeAliveSince < previousMsg.NodeAliveSince) || (selfNode.LastLogDate > previousMsg.LastLogDate))
                           )
                       )
                {                    
                    // Generate Full Sync
                    foreach (var dh in message.DataHash)
                    {
                        if (previousMsg.DataHash.FirstOrDefault(e => e.Id == dh.Id).Hash != dh.Hash)
                        {
                            ConfigServerTenant tenant = _tenantManager.Tenants.FirstOrDefault(t => t.Id == dh.Id);
                            LogMessage lmsg = new LogMessage()
                            {
                                Created = DateTime.UtcNow,
                                LogId = selfNode.LastLogId,
                                Full = true,
                                Entity = tenant.StartEntityName,
                                TenantId = tenant.Id,
                                JsonDiff = tenant.GetRaw("", tenant.StartEntityName).Value.ToString()
                            };
                            message.Log.Add(lmsg);
                        }
                    }
                }
            }
            return message;
        }
        private void ResponseFromHeartBeat(Node node, HeartBeatMessage msg)
        {
            if (msg.MessageType == HeartBeartType.HeartBeatResponse)
            {
                node.Attempts = 0;
                node.SkipAttempts = 0;
                node.ResetLife();

                if (
                      (
                        (selfNode.LastLogId == msg.LastLogId) &&
                        ((_AliveSince < msg.NodeAliveSince) || (selfNode.LastLogDate > msg.LastLogDate))
                      )
                      || (msg.LastLogId < selfNode.LastLogId))
                {
                    var req = CreateMessageFor(HeartBeartType.UpdateRequest, msg);
                    if (req.Log.Count > 0)
                    {
                        node.InUse = false;
                        SendHeartBeat(node, req);
                    }
                }
            }

            // Se supone que ya esto nunca va a ocurrir.
            else
            if (msg.MessageType == HeartBeartType.UpdateRequest)
            {
                node.Attempts = 0;
                node.SkipAttempts = 0;
                node.ResetLife();
                _tenantManager.ApplyUpdate(msg.Log, selfNode.LogMessages);
            }
            // ******************************************
        }
        private void SendHeartBeat(Node node, HeartBeatMessage req)
        {
            lock (node)
            {
                if (!node.Active) return;
                if ((node.Self) || (node.Mode == Node.NodeMode.Client) || (!node.NodeConfig.HeartBeat)) return;
                if (node.InUse)
                {
                    _logger.LogTrace(req.MessageType + " " + selfNode.Id + " Log(" + selfNode.LastLogId + ") --> X " + node.Id + "(InUse).");
                    return;
                }                
                if (node.SkipAttempts > 0)
                {
                    node.SkipAttempts--;
                    return;
                }
                lock (selfNode)
                {
                    if (selfNode.InUse)
                    {
                        _logger.LogTrace(req.MessageType + " " + selfNode.Id + "(inUse) Log(" + selfNode.LastLogId + ") --> X " + node.Id + ".");
                        return;
                    }
                    selfNode.InUse = true;
                }
                try
                {
                    HttpClient client = new HttpClient();
                    string msgData = JsonConvert.SerializeObject(req, Formatting.None);
                    node.InUse = true;
                    if (req.Log.Count > 0)
                        _logger.LogTrace(req.MessageType + " " + selfNode.Id + " Log(" + selfNode.LastLogId + ") --> " + node.Id + "." + (req.Log[0].Full ? "FULL " : "") + "sync for: " + req.Log.Count + " tenants.");
                    else
                        _logger.LogTrace(req.MessageType + " " + selfNode.Id + " Log(" + selfNode.LastLogId + ") --> " + node.Id + ".");

                    string uri = (node.DataCenter != selfNode.DataCenter) ? node.WANUri : node.Uri;
                    client.PostAsync(uri + "/api/Cluster/HeartBeat", new StringContent(msgData, Encoding.UTF8, "application/json"), _clusterSecurity.AppId, _clusterSecurity.SecretKey, _clusterSecurity.Encrypted).ContinueWith((a) =>
                    {
                        lock (node)
                        {
                            try
                            {
                                if ((a.Status == TaskStatus.RanToCompletion) && (a.Result.IsSuccessStatusCode))
                                {
                                    ApiSingleResult<HeartBeatMessage> rsMsg = JsonConvert.DeserializeObject<ApiSingleResult<HeartBeatMessage>>(a.Result.Content.ReadAsStringAsync().Result);
                                    if ((rsMsg == null) || (rsMsg.Item == null))
                                    {
                                        node.LastCheckActive = false;
                                        _logger.LogError("Node: " + node.Id + " return Invalid response. attempt: " + node.Attempts);
                                        node.Attempts++;
                                        node.Priority--;
                                    }
                                    else
                                    {
                                        node.LastCheckActive = true;
                                        _logger.LogTrace(rsMsg.Item.MessageType + " " + selfNode.Id + " Log(" + selfNode.LastLogId + ") <-- " + node.Id + " Log(" + rsMsg.Item.LastLogId + ")");
                                        ResponseFromHeartBeat(node, rsMsg.Item);
                                    }
                                }
                                else
                                {
                                    node.LastCheckActive = false;
                                    node.Attempts++;
                                    node.Priority--;
                                    if (a.Status == TaskStatus.RanToCompletion)
                                        _logger.LogError("Error contacting: " + node.Id + " (" + a.Result != null ? a.Result.StatusCode.ToString() : "" + ") attempt: " + node.Attempts);
                                    else
                                        _logger.LogError("Unable to contact: " + node.Id + " (" + a.Status.ToString() + ") attempt: " + node.Attempts);
                                }

                            }
                            catch (Exception ex)
                            {
                                node.LastCheckActive = false;
                                node.Attempts++;
                                node.Priority--;
                                _logger.LogCritical("Exception heartbeating node: " + node.Id + "\r\n" + ex.ToString());
                            }
                            finally
                            {
                                node.InUse = false;
                                lock(selfNode) selfNode.InUse = false;

                                #region Check for max lives
                                try
                                {
                                    if (node.Attempts >= _NodesMonitoringMaxAttempts)
                                    {
                                        if (node.Life == 0)
                                        {
                                            _cfgChanger.DisableClusterNode(node.Id);
                                            node.Active = false;
                                            node.LastCheckActive = false;
                                            node.Attempts = 0;
                                            _logger.LogError("Node " + node.Id + " life out; Disabled. ");
                                        }
                                        else
                                        {
                                            node.SkipAttempts = _NodesMonitoringSkipAttemptsOnFail;
                                            _logger.LogError("Heartbeat failed to " + node.Id + " " + node.Attempts + " attempts; Skip: " + node.SkipAttempts);
                                            node.Attempts = 0;
                                            node.Life--;
                                            node.LastCheckActive = false;
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogCritical("Exception heartbeating node: " + node.Id + "\r\n" + ex.ToString());
                                }
                                #endregion                                
                            }
                        }
                    });
                }
                catch (Exception ex)
                {
                    lock(selfNode) selfNode.InUse = false;
                    node.InUse = false;
                    _logger.LogCritical("Exception processing node: " + node.Id + ex.ToString());
                }
            }
        }
        public override HeartBeatMessage ReceiveHeartBeat(HeartBeatMessage msg)
        {
            #region Preconditions to Accept the heart beat (Node ans Selfnode are not in use)
            var node = Nodes.FirstOrDefault(n => n.Id == msg.NodeId);
            lock (selfNode)
            {
                if (selfNode.InUse)
                {
                    _logger.LogTrace(msg.MessageType + " " + selfNode.Id + "(InUse) Log(" + selfNode.LastLogId + ") X <-- " + msg.NodeId + " Log(" + msg.LastLogId + ")");
                    var response = CreateMessageFor(HeartBeartType.HeartBeatResponse, msg);
                    return response;
                }
            }
            if (node !=null)
                lock (node)
                {
                    if (node.InUse)
                    {
                        _logger.LogTrace(msg.MessageType + " " + selfNode.Id + " Log(" + selfNode.LastLogId + ") <-- " + msg.NodeId + "(InUse) Log(" + msg.LastLogId + ")");
                        var response = CreateMessageFor(HeartBeartType.HeartBeatResponse, msg);
                        return response;
                    }
                    node.InUse = true;
                }
            #endregion

            try
            {
                _logger.LogTrace(msg.MessageType + " " + selfNode.Id + " Log(" + selfNode.LastLogId + ") <-- " + msg.NodeId + " Log(" + msg.LastLogId + ")");
                if (msg.MessageType == HeartBeartType.UpdateRequest)
                {
                    // El originador envia una serie de logs en orden de aplicacion, estos logs se tienen que mergear con los
                    // logs de este nodo y aplicarlos en el orden de menor a mayor.
                    _tenantManager.ApplyUpdate(msg.Log, selfNode.LogMessages);

                    // En el updateResponse se debe devolver todos los logs que el originador del heartbeat no tiene, 
                    // para que este los aplique igualmente
                    var response = CreateMessageFor(HeartBeartType.UpdateResponse, msg);
                    return response;
                }
                else
                if (msg.MessageType == HeartBeartType.HeartBeatRequest)
                {
                    // se debe comparar si el logid no es el mismo (no importa si es mayor o menor) 
                    // entonces se devuelven todos los logsid que se han aplicado (solo el id)
                    // El originador debera tomar estos logsid y compararlos con lo que el tiene y mandar entonces
                    // el UpdateRequest con solo los logs que este nodo no tiene aplicados.
                    if (
                         (
                           (selfNode.LastLogId == msg.LastLogId) &&
                           ((_AliveSince < msg.NodeAliveSince) || (selfNode.LastLogDate > msg.LastLogDate))
                         )
                         || (msg.LastLogId < selfNode.LastLogId))
                    {
                        var response = CreateMessageFor(HeartBeartType.UpdateRequest, msg);
                        if (response.Log.Count > 0)
                        {
                            _logger.LogInformation(response.MessageType + " --> " + msg.NodeId + ". " + (response.Log[0].Full ? "FULL " : "") + "sync for: " + response.Log.Count + " tenants.");
                            return response;
                        }
                        else
                        {
                            response = CreateMessageFor(HeartBeartType.HeartBeatResponse, msg);
                            return response;
                        }
                    }
                    else
                    {
                        var response = CreateMessageFor(HeartBeartType.HeartBeatResponse, msg);
                        return response;
                    }
                }
                else
                {
                    var response = CreateMessageFor(HeartBeartType.Unknow, msg);
                    _logger.LogError("Unsupported Message: " + msg.MessageType + " received from " + msg.NodeId);
                    return response;
                }
            }
            finally
            {
                if (node != null)
                    lock (node)
                        node.InUse = false;

            }
        }

    }
}
