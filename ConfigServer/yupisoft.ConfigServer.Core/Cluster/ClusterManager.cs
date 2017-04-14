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

    public class ClusterManager
    {
        private object _lock = new object();
        private Timer _timer;
        private ILogger _logger;
        private ConfigurationChanger _cfgChanger;
        private ConfigServerManager _cfgServer;
        private int _NodesMonitoringHeartbeat = 2000; // Milliseconds
        private int _NodesMonitoringMaxAttempts = 3;
        private int _NodesMonitoringSkipAttemptsOnFail = 3;
        private int _NodesMonitoringLife = 3;
        private HmacAuthenticationOptions _clusterSecurity;
        private List<Node> _nodes;

        private SelfNode selfNode
        {
            get
            {
                lock (_nodes)
                {
                    if (_nodes == null) return null;
                    return _nodes.FirstOrDefault(n => n.Self) as SelfNode;
                }
            }
        }
        

        public ClusterManager(IOptions<ClusterConfigSection> clusterConfig, ILogger<ClusterManager> logger, ConfigurationChanger cfgChanger, ConfigServerManager cfgServer)
        {
            _cfgChanger = cfgChanger;
            _cfgServer = cfgServer;
            _cfgServer.DataChanged += _cfgServer_DataChanged;
            _logger = logger;
            _nodes = new List<Node>();
            _NodesMonitoringHeartbeat = clusterConfig.Value.Monitoring.Interval;
            _NodesMonitoringMaxAttempts = clusterConfig.Value.Monitoring.MaxAttempts;
            _NodesMonitoringSkipAttemptsOnFail = clusterConfig.Value.Monitoring.SkipAttemptsOnFail;
            _NodesMonitoringLife = clusterConfig.Value.Monitoring.NodesLife;
            _clusterSecurity = clusterConfig.Value.Security;

            var nodesConfig = clusterConfig.Value.Nodes;
            foreach(var node in nodesConfig)
            {
                if (node.Enabled)
                {
                    Node newNode = null;
                    if (clusterConfig.Value.OwnNodeName == (node.Id))
                        newNode = new SelfNode() { Id = node.Id, Active = true, Address = node.Address, NodeConfig = node };
                    else
                        newNode = new Node() { Id = node.Id, Active = true, Address = node.Address, NodeConfig = node };

                    _nodes.Add(newNode);
                }
            }

            if (selfNode == null)
            {
                var newNode = new SelfNode() { Id = clusterConfig.Value.OwnNodeName, Active = true, Address = clusterConfig.Value.OwnNodeUrl, NodeConfig = new NodeConfigSection(){ Id = clusterConfig.Value.OwnNodeName, Address= clusterConfig.Value.OwnNodeUrl, Enabled = true } };
                _nodes.Add(newNode);
            }

            if (selfNode == null)
            {
                _logger.LogCritical("The current node (SelfNode) is not in the Nodes List.");
                Environment.Exit(1);
            }

            _timer = new Timer(new TimerCallback(Timer_Elapsed), _nodes, Timeout.Infinite, _NodesMonitoringHeartbeat);
            _logger.LogInformation("Created ClusterManager with " + _nodes.Count + " nodes.");
        }

        public HeartBeatMessage CreateMessageFor(HeartBeartType type, HeartBeatMessage msg)
        {
            HeartBeatMessage message = new HeartBeatMessage();
            message.Created = DateTime.UtcNow;
            message.NodeId = selfNode.Id;
            message.MessageType = type;
            message.NodeMode = selfNode.Mode;

            if (type == HeartBeartType.Unknow) return message;

            message.LastLogDate = selfNode.LastLogDate;
            message.LastLogId = selfNode.LastLogId;
            message.NodeAliveSince = _cfgServer.AliveSince;
            message.DataHash = _cfgServer.TenantManager.Tenants.Select(p => new TenantHash() { Id = p.TenantConfig.Id, Hash = p.DataHash }).ToArray();
            if ((type == HeartBeartType.HeartBeatRequest) || (type == HeartBeartType.HeartBeatResponse) || (type == HeartBeartType.HeartBeatUpdateResponse) || (type == HeartBeartType.UpdateResponse))
            {
                message.Nodes = _nodes.Select(n => n.NodeConfig.Serialize()).ToArray();
            }
            if ((type == HeartBeartType.UpdateRequest))
            {
                #region message.Nodes --> Determine Discovery Node Collection Diferences
                // Check diferences, send only the diferences
                var currentNodes = _nodes.Select(n => n.NodeConfig).ToArray();
                var msgNodes = msg.Nodes.Select(n => NodeConfigSection.Deserialize(n)).ToList();
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
                                if (_cfgServer.AliveSince > msg.NodeAliveSince)
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

                bool DifferentDataHash = false;
                foreach (var dh in message.DataHash)
                    if (msg.DataHash.FirstOrDefault(e => e.Id == dh.Id).Hash != dh.Hash)
                    {
                        DifferentDataHash = true;
                        break;
                    }
                if ((!DifferentDataHash) && (msg.LastLogId < message.LastLogId))
                {
                    LogMessage lmsg = new LogMessage()  // This is a fake upddate.
                    {
                        Created = DateTime.UtcNow,
                        LogId = selfNode.LastLogId,
                        Full = false,
                        Entity = null,
                        TenantId = null,
                        JsonDiff = null
                    };
                    message.Log.Add(lmsg);
                    message.LastLogId = selfNode.LastLogId;
                    message.LastLogDate = selfNode.LastLogDate;
                }
                else
                if ((msg.LastLogId < message.LastLogId) && (msg.LastLogId > 0))
                {
                    var logMessages = selfNode.LogMessages.FirstOrDefault(m => m.LogId > msg.LastLogId);
                    message.Log.Add(logMessages);
                    message.LastLogId = selfNode.LastLogId;
                    message.LastLogDate = selfNode.LastLogDate;
                }
                else
                    if (DifferentDataHash &&
                       ((selfNode.LastLogId == msg.LastLogId)
                         && (((message.NodeAliveSince < msg.NodeAliveSince)) || ((message.LastLogId > 0) && (message.LastLogDate > msg.LastLogDate))))
                     || ((msg.LastLogId == 0) && (msg.LastLogId < message.LastLogId)))
                {
                    foreach (var dh in message.DataHash)
                    {
                        if (msg.DataHash.FirstOrDefault(e => e.Id == dh.Id).Hash != dh.Hash)
                        {
                            ConfigServerTenant tenant = _cfgServer.TenantManager.Tenants.FirstOrDefault(t => t.Id == dh.Id);
                            LogMessage lmsg = new LogMessage()
                            {
                                Created = DateTime.UtcNow,
                                LogId = selfNode.LastLogId,
                                Full = true,
                                Entity = tenant.StartEntityName,
                                TenantId = tenant.Id,
                                JsonDiff = _cfgServer.GetRaw("", tenant.StartEntityName, tenant.Id).Value.ToString()
                            };
                            //selfNode.LogMessages.Add(lmsg);
                            message.Log.Add(lmsg);
                        }
                    }
                }
            }
            return message;
        }

        public HeartBeatMessage CreateMessageFor1(HeartBeartType type, HeartBeatMessage msg)
        {
            HeartBeatMessage message = new HeartBeatMessage();
            message.Created = DateTime.UtcNow;
            message.NodeId = selfNode.Id;
            message.MessageType = type;
            message.NodeMode = selfNode.Mode;

            if (type == HeartBeartType.Unknow) return message;

            message.LastLogDate = selfNode.LastLogDate;
            message.LastLogId = selfNode.LastLogId;
            message.NodeAliveSince = _cfgServer.AliveSince;            
            message.DataHash = _cfgServer.TenantManager.Tenants.Select(p => new TenantHash() { Id = p.TenantConfig.Id, Hash = p.DataHash }).ToArray();
            if ((type == HeartBeartType.HeartBeatRequest) || (type == HeartBeartType.HeartBeatResponse) || (type == HeartBeartType.HeartBeatUpdateResponse) || (type == HeartBeartType.UpdateResponse))
            {
                message.Nodes = _nodes.Select(n => n.NodeConfig.Serialize()).ToArray();
            }
            if ((type == HeartBeartType.UpdateRequest))
            {
                #region message.Nodes --> Determine Discovery Node Collection Diferences
                // Check diferences, send only the diferences
                var currentNodes = _nodes.Select(n => n.NodeConfig).ToArray();
                var msgNodes = msg.Nodes.Select(n => NodeConfigSection.Deserialize(n)).ToList();
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
                                if (_cfgServer.AliveSince > msg.NodeAliveSince)
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

                bool DifferentDataHash = false;
                foreach (var dh in message.DataHash)
                    if (msg.DataHash.FirstOrDefault(e => e.Id == dh.Id).Hash != dh.Hash)
                    {
                        DifferentDataHash = true;
                        break;
                    }

                if ((msg.LastLogId < message.LastLogId) && (selfNode.LastLogId > 0))
                {
                    var logMessages = selfNode.LogMessages.FirstOrDefault(m => m.LogId > msg.LastLogId);
                    message.Log.Add(logMessages);
                    message.LastLogId = selfNode.LastLogId;
                    message.LastLogDate = selfNode.LastLogDate;
                }
                else
                    if (DifferentDataHash && 
                       ((selfNode.LastLogId == msg.LastLogId)
                         && (((message.NodeAliveSince < msg.NodeAliveSince)) || ((message.LastLogId > 0) && (message.LastLogDate > msg.LastLogDate))))
                     ||((selfNode.LastLogId == 0) && (msg.LastLogId < message.LastLogId)))
                    {
                        foreach (var dh in message.DataHash)
                        {
                            if (msg.DataHash.FirstOrDefault(e => e.Id == dh.Id).Hash != dh.Hash)
                            {
                                ConfigServerTenant tenant = _cfgServer.TenantManager.Tenants.FirstOrDefault(t => t.Id == dh.Id);
                                LogMessage lmsg = new LogMessage()
                                {
                                    Created = DateTime.UtcNow,
                                    LogId = selfNode.LastLogId,
                                    Full = true,
                                    Entity = tenant.StartEntityName,
                                    TenantId = tenant.Id,
                                    JsonDiff = _cfgServer.GetRaw("", tenant.StartEntityName, tenant.Id).Value.ToString()
                                };
                                //selfNode.LogMessages.Add(lmsg);
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

                if ( (
                      (selfNode.LastLogId == msg.LastLogId) &&
                      ((_cfgServer.AliveSince < msg.NodeAliveSince) || ((selfNode.LastLogId > 0) && (selfNode.LastLogDate > msg.LastLogDate)))
                     )
                     || (msg.LastLogId < selfNode.LastLogId)
                   )
                {
                    var req = CreateMessageFor(HeartBeartType.UpdateRequest, msg);
                    if (req.Log.Count > 0)
                    {
                        node.InUse = false;
                        SendHeartBeat(node, req);
                    }
                }
            }
            else
            if (msg.MessageType == HeartBeartType.UpdateRequest)
            {
                node.Attempts = 0;
                node.SkipAttempts = 0;
                node.ResetLife();
                foreach (var log in msg.Log)
                {
                    bool applied = false;
                    if (log.TenantId == null)
                    {
                        applied = true;
                        _logger.LogTrace("<Empty log>");
                    }
                    else
                    {
                        if (log.Full)
                            applied = _cfgServer.Set(new JNode("", JToken.Parse(log.JsonDiff), log.Entity), log.TenantId, true);
                        else
                            applied = _cfgServer.ApplyUpdate(log.TenantId, log.Entity, log.JsonDiff);
                    }
                    if (applied) selfNode.LogMessages.Add(log);
                }
                _logger.LogTrace("Applied " + msg.Log.Count() + " logs successfully.");
            }
        }

        public HeartBeatMessage ReceiveHeartBeat(HeartBeatMessage msg)
        {
            _logger.LogTrace(msg.MessageType + " " + selfNode.Id + " Log(" + selfNode.LastLogId + ") <-- " + msg.NodeId + " Log(" + msg.LastLogId + ")");
            if (msg.MessageType == HeartBeartType.UpdateRequest)
            {
                // Perform the updates
                foreach (var log in msg.Log)
                {
                    bool applied = false;
                    if (log.TenantId == null)
                    {
                        applied = true;
                        _logger.LogTrace("<Empty log>");
                    }
                    else
                    {
                        if (log.Full)
                            applied = _cfgServer.Set(new JNode("", JToken.Parse(log.JsonDiff), log.Entity), log.TenantId, true);
                        else
                            applied = _cfgServer.ApplyUpdate(log.TenantId, log.Entity, log.JsonDiff);
                    }
                    if (applied) selfNode.LogMessages.Add(log);
                }
                _logger.LogTrace("Applied " + msg.Log.Count() + " logs successfully.");

                var response = CreateMessageFor(HeartBeartType.UpdateResponse, msg);
                return response;
            }
            else
            if (msg.MessageType == HeartBeartType.HeartBeatRequest)
            {
                if (
                     (
                       (selfNode.LastLogId == msg.LastLogId) && 
                       ((_cfgServer.AliveSince < msg.NodeAliveSince) || ((selfNode.LastLogId > 0) && (selfNode.LastLogDate > msg.LastLogDate)))
                     )
                     || (msg.LastLogId < selfNode.LastLogId))
                {
                    var response = CreateMessageFor(HeartBeartType.UpdateRequest, msg);
                    if (response.Log.Count > 0)
                    {
                        _logger.LogTrace(response.MessageType + " --> " + msg.NodeId + ". " + (response.Log[0].Full ? "FULL " : "") + "sync for: " + response.Log.Count + " tenants.");
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

        private void SendHeartBeat(Node node, HeartBeatMessage req)
        {
            lock (node)
            {
                if (!node.Active) return;
                if ((node.Self) || (node.InUse) || (node.Mode == Node.NodeMode.Client) || (!node.NodeConfig.HeartBeat)) return;
                //if ((node.InUse) || (node.Mode == Node.NodeMode.Client)) return;
                if (node.SkipAttempts > 0)
                {
                    node.SkipAttempts--;
                    return;
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
                    client.PostAsync(node.Address + "/api/Cluster/HeartBeat", new StringContent(msgData, Encoding.UTF8, "application/json"), _clusterSecurity.AppId, _clusterSecurity.SecretKey, _clusterSecurity.Encrypted).ContinueWith((a) =>
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
                                        _logger.LogError("Node: " + node.Id + " return Invalid response. attempt: " + node.Attempts);
                                        node.Attempts++;
                                        node.Priority--;
                                    }
                                    else
                                    {
                                        _logger.LogTrace(rsMsg.Item.MessageType + " " + selfNode.Id + " Log(" + selfNode.LastLogId + ") <-- " + node.Id + " Log(" + rsMsg.Item.LastLogId + ")" );
                                        ResponseFromHeartBeat(node, rsMsg.Item);
                                    }
                                }
                                else
                                {
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
                                node.Attempts++;
                                node.Priority--;
                                _logger.LogCritical("Exception heartbeating node: " + node.Id + ex.ToString());
                            }
                            finally
                            {
                                node.InUse = false;

                                #region Check for max lives

                                if (node.Attempts >= _NodesMonitoringMaxAttempts)
                                {
                                    if (node.Life == 0)
                                    {
                                        _cfgChanger.DisableClusterNode(node.Id);
                                        node.Active = false;
                                        node.Attempts = 0;
                                        _logger.LogError("Node " + node.Id + " life out; Disabled. ");
                                    }
                                    else
                                    {
                                        node.SkipAttempts = _NodesMonitoringSkipAttemptsOnFail;
                                        _logger.LogError("Heartbeat failed to " + node.Id + " " + node.Attempts + " attempts; Skip: " + node.SkipAttempts);
                                        node.Attempts = 0;
                                        node.Life--;
                                    }
                                }

                                #endregion                                
                            }
                        }
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogCritical("Exception processing node: " + node.Id + ex.ToString());
                }                
            }
        }



        private void _cfgServer_DataChanged(string tenantId, string entity, JToken diffToken)
        {
            _logger.LogInformation("Data changed Tenant " + tenantId + " entity: " + entity + " diff:" + diffToken?.ToString());

            LogMessage lm = new LogMessage() {Created = DateTime.UtcNow, Entity = entity, TenantId = tenantId, JsonDiff = diffToken?.ToString(Formatting.None) };
            lock (selfNode)
            {                
                lm.LogId = (selfNode.LogMessages.Count > 0) ? (selfNode.LogMessages.Last().LogId + 1) : 1;
                selfNode.LogMessages.Add(lm);
            }
        }

        //#region To change

        ///// <summary>
        ///// Compare DataHash from Request to our registered dataHash and return the tenants that are diferents.
        ///// </summary>
        ///// <param name="dataHash"></param>
        ///// <returns></returns>
        //private List<string> TenantsToUpgrade(KeyValuePair<string, string>[] dataHash)
        //{
        //    List<string> TenantsToUpgrade = new List<string>();
        //    var DataHash = _cfgServer.TenantManager.Tenants.Select(p => new KeyValuePair<string, string>(p.TenantConfig.Id, p.DataHash)).ToArray();
        //    // Check DataHash
        //    foreach (var dh in DataHash)
        //    {
        //        if (dataHash.FirstOrDefault(e => e.Key == dh.Key).Value != dh.Value)
        //            TenantsToUpgrade.Add(dh.Key);
        //    }
        //    return TenantsToUpgrade;
        //}

        ///// <summary>
        ///// Respond to a HeartBeat Request Message
        ///// </summary>
        ///// <param name="request"></param>
        ///// <returns></returns>
        //public HeartBeatMessageResponse ProcessHeartBeatRequest(HeartBeatMessageRequest request)
        //{

        //    lock (selfNode)
        //    {
        //        HeartBeatMessageResponse response = new HeartBeatMessageResponse();
        //        response.Created = DateTime.UtcNow;
        //        response.NodeAliveSince = _cfgServer.AliveSince;
        //        response.LastLogId = selfNode.LastLogId;
        //        response.NodeId = selfNode.Id;
        //        if (request == null)
        //        {
        //            _logger.LogTrace("Null message; Ignoring HeartBeat. ");
        //            response.Command = HeartBeartCommand.Unknow;
        //            response.Result = HeartBeartCommandResult.Error;
        //            return response;
        //        }

        //        UpdateNodes(request.Nodes);

        //        if (selfNode.InUse)
        //        {
        //            _logger.LogTrace("Node in Use; Ignoring HeartBeat: " + request.Command);
        //            response.Command = HeartBeartCommand.Abort;
        //            response.Result = HeartBeartCommandResult.InUse;
        //            return response;
        //        }

        //        if (request.Command == HeartBeartCommand.SyncRequest)
        //        {
        //            _logger.LogTrace("SyncRequest received from Node: " + request.NodeId);
        //            response.Command = HeartBeartCommand.SyncResponse;

        //            response.Log = selfNode.LogMessages.Where(l => l.LogId > request.LastLogId).ToList();
        //            return response;
        //        }

        //        if (request.Command == HeartBeartCommand.FullSyncRequest)
        //        {
        //            _logger.LogTrace("FullSyncRequest received from Node: " + request.NodeId);
        //            response.Command = HeartBeartCommand.FullSyncResponse;
        //            if (request.Log.Count > 0)
        //            {
        //                List<LogMessage> logToUpdate = new List<LogMessage>();
        //                foreach (LogMessage msg in request.Log)
        //                {
        //                    ConfigServerTenant tenant = _cfgServer.TenantManager.Tenants.FirstOrDefault(t => t.TenantConfig.Id == msg.TenantId);
        //                    LogMessage lmsg = new LogMessage()
        //                    {
        //                        Created = DateTime.UtcNow,
        //                        LogId = selfNode.LastLogId,
        //                        Full = true,
        //                        Entity = tenant.StartEntityName,
        //                        TenantId = tenant.TenantConfig.Id,
        //                        JsonDiff = _cfgServer.GetRaw("", tenant.StartEntityName, tenant.TenantConfig.Id).Value.ToString()
        //                    };
        //                    logToUpdate.Add(lmsg);
        //                }
        //                response.Log = logToUpdate;
        //            }
        //            return response;
        //        }

        //        if (request.Command == HeartBeartCommand.HeartBeatRequest)
        //        {
        //            _logger.LogTrace("HeartBeatRequest received from Node: " + request.NodeId);
        //            response.Command = HeartBeartCommand.HeartBeatResponse;
        //            // Si el nodo tiene Log 0 y fue creado despues que el nodo que hizo el request, deberemos comprobar el hash
        //            if ((selfNode.LastLogId == 0) && (_cfgServer.AliveSince > request.NodeAliveSince))
        //            {
        //                // Hay que verificar que el request node no sea un Client Node
        //                // o
        //                // Lo otro es que en ves de pedirle una sincronizacion con un request,
        //                // podria responderle que me mande una sinc en el proximo heartbeat  (Creo que esto seria mejor)

        //                List<string> upgrade = TenantsToUpgrade(request.DataHash);
        //                if (upgrade.Count > 0)
        //                {
        //                    _logger.LogTrace("Self Node needs full sync from: " + request.NodeId + " for: " + upgrade.Count + " tenants.");

        //                    if (request.NodeMode == Node.NodeMode.Server)
        //                        SendHeartBeatSyncRequest(request.NodeId, response, true, upgrade.ToArray());
        //                    else
        //                    {
        //                        _logger.LogTrace("Node: " + request.NodeId + " is a client Node, respond as a fullsync request.");
        //                        response.Command = HeartBeartCommand.FullSyncRequest;
        //                        foreach (string t in upgrade)
        //                            response.Log.Add(new LogMessage() { Created = DateTime.UtcNow, Entity = "default", TenantId = t, JsonDiff = null, LogId = 0 });
        //                    }
        //                }
        //            }
        //            else
        //            if (selfNode.LastLogId > request.LastLogId) // My LogId is updated
        //            {
        //                _logger.LogTrace("Remote Node: " + request.NodeId + " needs upgrade, SelfLog: " + selfNode.LastLogId + " versus:" + request.LastLogId);
        //                // No hace nada porque deja que el nodo desactualizado se de cuenta que necesita actualizarse, cuando haga su heartbeat.
        //            }
        //            else
        //            if (selfNode.LastLogId < request.LastLogId) // My LogId is not updated
        //            {
        //                try
        //                {
        //                    _logger.LogTrace("Self Node needs upgrade from: " + request.NodeId + ", SelfLog: " + selfNode.LastLogId + " versus:" + request.LastLogId);
        //                    if (request.NodeMode == Node.NodeMode.Server)
        //                        SendHeartBeatSyncRequest(request.NodeId, response, false);
        //                    else
        //                    {
        //                        _logger.LogTrace("Node: " + request.NodeId + " is a client Node, respond as a fullsync request.");
        //                        response.Command = HeartBeartCommand.SyncRequest;
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    response.Result = HeartBeartCommandResult.Error;
        //                    _logger.LogCritical("Exception procesing node: " + request.NodeId + ex.ToString());
        //                }
        //            }
        //            else
        //            if ((selfNode.LastLogId == request.LastLogId) && (selfNode.LastLogId > 0) && (selfNode.LastLogDate < request.LastLogDate))
        //            {
        //                try
        //                {
        //                    List<string> upgrade = TenantsToUpgrade(request.DataHash);

        //                    if (upgrade.Count > 0)
        //                    {
        //                        _logger.LogTrace("Self Node needs full sync from: " + request.NodeId + " for: " + upgrade.Count + " tenants.");

        //                        if (request.NodeMode == Node.NodeMode.Server)
        //                            SendHeartBeatSyncRequest(request.NodeId, response, true, upgrade.ToArray());
        //                        else
        //                        {
        //                            _logger.LogTrace("Node: " + request.NodeId + " is a client Node, respond as a fullsync request.");
        //                            response.Command = HeartBeartCommand.FullSyncRequest;
        //                            foreach (string t in upgrade)
        //                                response.Log.Add(new LogMessage() { Created = DateTime.UtcNow, Entity = "default", TenantId = t, JsonDiff = null, LogId = 0 });
        //                        }
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    response.Result = HeartBeartCommandResult.Error;
        //                    _logger.LogCritical("Exception procesing node: " + request.NodeId + ex.ToString());
        //                }
        //            }
        //            else
        //            {
        //                //_logger.LogCritical("Nothing to do to handle node: " + request.NodeId);
        //                //if (_cfgServer.AliveSince > request.NodeAliveSince)
        //                //{
        //                //    // El nodo que esta haciendo el request fue creado antes, so, voy a verificar si el hash mio es igual que el de el.
        //                //    List<int> upgrade = TenantsToUpgrade(request.DataHash);

        //                //    if (upgrade.Count > 0)
        //                //    {
        //                //        selfNode.Status = SelfNodeStatus.Unsyncronized;
        //                //        HeartBeatSyncRequest(request.NodeId, response, true, upgrade.ToArray());
        //                //    }
        //                //}
        //            }
        //            return response;
        //        }

        //        response.Command = HeartBeartCommand.Unknow;
        //        response.Result = HeartBeartCommandResult.Error;
        //        _logger.LogTrace("Unknow command: " + request.Command.ToString());
        //        return response;
        //    }
        //}

        ///// <summary>
        ///// Send a HeartBeatSync Request to a node.
        ///// </summary>
        ///// <param name="NodeId"></param>
        ///// <param name="response"></param>
        ///// <param name="fullSync"></param>
        ///// <param name="tenants"></param>
        //private void SendHeartBeatSyncRequest(string NodeId, HeartBeatMessageResponse response, bool fullSync, string[] tenants = null)
        //{
        //    HeartBeatMessageRequest req = new HeartBeatMessageRequest();
        //    req.Command = (fullSync ? HeartBeartCommand.FullSyncRequest : HeartBeartCommand.SyncRequest);
        //    req.Created = DateTime.UtcNow;
        //    req.NodeId = selfNode.Id;
        //    req.NodeMode = selfNode.Mode;
        //    req.NodeAliveSince = _cfgServer.AliveSince;
        //    req.LastLogId = selfNode.LastLogId;
        //    req.LastLogDate = selfNode.LastLogDate;
        //    req.DataHash = _cfgServer.TenantManager.Tenants.Select(p => new KeyValuePair<string, string>(p.TenantConfig.Id, p.DataHash)).ToArray();

        //    lock (_nodes) req.Nodes = _nodes.Select(e => e.NodeConfig).ToArray();
        //    if (tenants != null)
        //    {
        //        foreach (string t in tenants)
        //        {
        //            req.Log.Add(new LogMessage() { Created = DateTime.UtcNow, Entity = "default", TenantId = t, JsonDiff = null, LogId = 0 });
        //        }
        //    }
        //    string msgData = JsonConvert.SerializeObject(req, Formatting.None);
        //    HttpClient client = new HttpClient();
        //    Node requestNode = null;

        //    lock (_nodes) { requestNode = _nodes.FirstOrDefault(n => n.Id == NodeId); }
        //    if (requestNode == null)
        //    {
        //        _logger.LogError("Node: " + NodeId + " not found in node list. Aborting upgrade.");
        //        response.Result = HeartBeartCommandResult.Error;
        //    }
        //    else
        //    {
        //        _logger.LogTrace((fullSync ? "FullSyncRequest" : "SyncRequest") + " --> " + requestNode.Id);
        //        response.Result = HeartBeartCommandResult.Success;
        //        selfNode.InUse = true;
        //        requestNode.InUse = true;
        //        client.PostAsync(requestNode.Address + "/api/Cluster/HeartBeat", new StringContent(msgData, Encoding.UTF8, "application/json"), _clusterSecurity.AppId, _clusterSecurity.SecretKey, _clusterSecurity.Encrypted).ContinueWith((a) =>
        //        {
        //            lock (selfNode)
        //            {
        //                try
        //                {
        //                    _logger.LogTrace("Received from " + requestNode.Id + " HeartBeat<Sync> ST:" + a.Status);
        //                    if ((a.Status == TaskStatus.RanToCompletion) && (a.Result.IsSuccessStatusCode))
        //                    {
        //                        ApiSingleResult<HeartBeatMessageResponse> rsMsg = JsonConvert.DeserializeObject<ApiSingleResult<HeartBeatMessageResponse>>(a.Result.Content.ReadAsStringAsync().Result);
        //                        if (rsMsg.Item == null)
        //                        {
        //                            _logger.LogError("Node: " + requestNode.Id + " do not return valid response. <null>");
        //                            return;
        //                        }

        //                        if (rsMsg.Item.Result != HeartBeartCommandResult.Success)
        //                        {
        //                            _logger.LogError("Node: " + requestNode.Id + " respond: " + rsMsg.Item.Result.ToString());
        //                            return;
        //                        }

        //                        if ((rsMsg.Item.Command != HeartBeartCommand.SyncResponse) && (rsMsg.Item.Command != HeartBeartCommand.FullSyncResponse))
        //                        {
        //                            _logger.LogError("Node: " + requestNode.Id + " do not return valid command: " + rsMsg.Item.Command.ToString());
        //                            return;
        //                        }

        //                        if (rsMsg.Item.Log == null)
        //                        {
        //                            _logger.LogError("Node: " + requestNode.Id + " do not return valid response <log null>. ");
        //                            return;
        //                        }

        //                        if (rsMsg.Item.Log.Count == 0)
        //                        {
        //                            _logger.LogError("Node: " + requestNode.Id + " do not return valid response <log empty>. ");
        //                            return;
        //                        }

        //                        IEnumerable<LogMessage> logsToApply = null;

        //                        if (rsMsg.Item.Command == HeartBeartCommand.SyncResponse)
        //                        {
        //                            logsToApply = rsMsg.Item.Log.Where(l => l.LogId > selfNode.LastLogId);
        //                            foreach (var log in logsToApply)
        //                            {
        //                                bool applied = false;
        //                                if (log.Full)
        //                                    applied = _cfgServer.Set(new JNode("", JToken.Parse(log.JsonDiff), log.Entity), log.TenantId, true);
        //                                else
        //                                    applied = _cfgServer.ApplyUpdate(log.TenantId, log.Entity, log.JsonDiff);
        //                                if (applied) selfNode.LogMessages.Add(log);
        //                            }
        //                            _logger.LogInformation("Applied " + logsToApply.Count() + " logs successfully.");
        //                        }

        //                        if (rsMsg.Item.Command == HeartBeartCommand.FullSyncResponse)
        //                        {
        //                            foreach (var log in rsMsg.Item.Log)
        //                            {
        //                                JNode node = new JNode("", JToken.Parse(log.JsonDiff), log.Entity);
        //                                bool applied = _cfgServer.Set(node, log.TenantId, true);
        //                                if (applied) selfNode.LogMessages.Add(log);
        //                            }
        //                            _logger.LogInformation("FullSync Applied successfully.");
        //                        }

        //                    }
        //                    else
        //                    {
        //                        _logger.LogError("Unable to contact: " + requestNode.Id + " for sync. ");
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    _logger.LogCritical("Exception procesing response from node: " + requestNode.Id + "\r\n" + ex.ToString());
        //                }
        //                finally
        //                {
        //                    requestNode.InUse = false;
        //                    selfNode.InUse = false;
        //                }
        //            }
        //        });
        //    }
        //}

        ///// <summary>
        ///// Update the current nodes array based on nodesConfig
        ///// </summary>
        ///// <param name="nodesConfig"></param>
        //private void UpdateNodes(NodeConfigSection[] nodesConfig)
        //{
        //    if (nodesConfig == null) return;
        //    lock (_nodes)
        //    {
        //        foreach (var node in nodesConfig)
        //        {

        //            var foundNode = _nodes.FirstOrDefault(e => e.Id == node.Id);
        //            if (foundNode != null)
        //            {
        //                if (node.Enabled)
        //                    foundNode.Active = true;
        //                else
        //                {
        //                    foundNode.Active = false;
        //                    _cfgChanger.DisableClusterNode(node.Id);
        //                }
        //            }
        //            else
        //            {
        //                var newNode = new Node() { Id = node.Id, Active = true, Address = node.Address, Self = false, NodeConfig = node };
        //                lock (_nodes)
        //                {
        //                    _logger.LogInformation("Adding New Node Id: " + newNode.Id + ".");
        //                    _nodes.Add(newNode);
        //                    _cfgChanger.AddClusterNode(node);
        //                }
        //            }
        //        }
        //    }
        //}

        ///// <summary>
        ///// Send HeartBeat to a node
        ///// </summary>
        ///// <param name="node"></param>
        //private void SendHeartBeat(Node node)
        //{
        //    if ((node.Self) || (node.InUse) || (node.Mode == Node.NodeMode.Client)) return;
        //    if (node.InUse) return;
        //    if (node.SkipAttempts > 0)
        //    {
        //        node.SkipAttempts--;
        //        return;
        //    }
        //    try
        //    {
        //        // ==========================================================================
        //        //  Create Request Message here.
        //        // ==========================================================================

        //        lock (selfNode)
        //        {
        //            HeartBeatMessageRequest request = new HeartBeatMessageRequest();
        //            request.Created = DateTime.UtcNow;
        //            request.Command = HeartBeartCommand.HeartBeatRequest;
        //            request.NodeId = selfNode.Id;
        //            request.NodeMode = selfNode.Mode;
        //            request.NodeAliveSince = _cfgServer.AliveSince;
        //            request.LastLogDate = selfNode.LastLogDate;
        //            request.LastLogId = selfNode.LastLogId;
        //            request.DataHash = _cfgServer.TenantManager.Tenants.Select(p => new KeyValuePair<string, string>(p.TenantConfig.Id, p.DataHash)).ToArray();

        //            // Only send in heartbeat info the server nodes.
        //            lock (_nodes) request.Nodes = _nodes.Where(n => n.Mode == Node.NodeMode.Server).Select(e => e.NodeConfig).ToArray();

        //            string msgData = JsonConvert.SerializeObject(request, Formatting.None);
        //            HttpClient client = new HttpClient();
        //            _logger.LogTrace("HeartBeat --> " + node.Id + " Hash:" + request.DataHash[0].Value + " Log:" + selfNode.LastLogId);
        //            node.InUse = true;

        //            client.PostAsync(node.Address + "/api/Cluster/Heartbeat", new StringContent(msgData, Encoding.UTF8, "application/json"), _clusterSecurity.AppId, _clusterSecurity.SecretKey, _clusterSecurity.Encrypted).ContinueWith((a) =>
        //            {
        //                lock (node)
        //                {
        //                    try
        //                    {
        //                        if ((a.Status == TaskStatus.RanToCompletion) && (a.Result.IsSuccessStatusCode))
        //                        {
        //                            ApiSingleResult<HeartBeatMessageResponse> rsMsg = JsonConvert.DeserializeObject<ApiSingleResult<HeartBeatMessageResponse>>(a.Result.Content.ReadAsStringAsync().Result);
        //                            if ((rsMsg == null) || (rsMsg.Item == null))
        //                            {
        //                                _logger.LogError("Node: " + node.Id + " return Invalid response. attempt: " + node.Attempts);
        //                                node.Attempts++;
        //                            }
        //                            else
        //                            if (rsMsg.Item.Result != HeartBeartCommandResult.Success)
        //                            {
        //                                _logger.LogError("Node: " + node.Id + " respond: " + rsMsg.Item.Result.ToString());
        //                                node.Attempts++;
        //                            }
        //                            else
        //                            {
        //                                if (rsMsg.Item.Command == HeartBeartCommand.HeartBeatResponse)
        //                                {
        //                                    _logger.LogInformation("Heartbeat from " + node.Id + " successfully.");
        //                                    node.Attempts = 0;
        //                                    node.SkipAttempts = 0;
        //                                    node.ResetLife();
        //                                }
        //                                if (rsMsg.Item.Command == HeartBeartCommand.FullSyncRequest)
        //                                {
        //                                    _logger.LogTrace("SyncRequest received from Node: " + rsMsg.Item.NodeId);
        //                                    response.Command = HeartBeartCommand.SyncResponse;
        //                                    response.Log = selfNode.LogMessages.Where(l => l.LogId > request.LastLogId).ToList();
        //                                }
        //                                if (rsMsg.Item.Command == HeartBeartCommand.SyncRequest)
        //                                {

        //                                }
        //                            }
        //                        }
        //                        else
        //                        {
        //                            node.Attempts++;
        //                            node.Priority--;
        //                            if (a.Status == TaskStatus.RanToCompletion)
        //                                _logger.LogError("Error contacting: " + node.Id + " (" + a.Result != null ? a.Result.StatusCode.ToString() : "" + ") attempt: " + node.Attempts);
        //                            else
        //                                _logger.LogError("Unable to contact: " + node.Id + " (" + a.Status.ToString() + ") attempt: " + node.Attempts);
        //                        }

        //                        if (node.Attempts >= _NodesMonitoringMaxAttempts)
        //                        {
        //                            if (node.Life == 0)
        //                            {
        //                                lock (_cfgChanger)
        //                                {
        //                                    _cfgChanger.DisableClusterNode(node.Id);
        //                                    node.Active = false;
        //                                }
        //                                node.Attempts = 0;
        //                                _logger.LogError("Node " + node.Id + " life out; Disabled. ");
        //                            }
        //                            else
        //                            {
        //                                node.SkipAttempts = _NodesMonitoringSkipAttemptsOnFail;
        //                                _logger.LogError("Heartbeat failed to " + node.Id + " " + node.Attempts + " attempts; Skip: " + node.SkipAttempts);
        //                                node.Attempts = 0;
        //                                node.Life--;
        //                            }
        //                        }
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        _logger.LogCritical("Exception heartbeating node: " + node.Id + ex.ToString());
        //                    }
        //                    finally
        //                    {
        //                        node.InUse = false;
        //                    }
        //                }
        //            });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogCritical("Exception processing node: " + node.Id + ex.ToString());
        //    }

        //}

        //#endregion

        private void Timer_Elapsed(object state)
        {
            _timer.Change(Timeout.Infinite, _NodesMonitoringHeartbeat); // Disable the timer;
            if (selfNode.Mode == Node.NodeMode.Server)
            {
                Node[] nodes = null;
                lock (_nodes){nodes = _nodes.ToArray();}
                foreach (var w in nodes)
                {
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
                lock (_nodes){
                    node = _nodes.OrderByDescending(n => n.Priority).FirstOrDefault(n => !n.Self && n.Active && n.Mode == Node.NodeMode.Server);
                }
                var req = CreateMessageFor(HeartBeartType.HeartBeatRequest, null);
                SendHeartBeat(node, req);                
            }
            _timer.Change(_NodesMonitoringHeartbeat, _NodesMonitoringHeartbeat); // Reenable the timer;
        }

        public void StartManaging()
        {
            _timer.Change(Timeout.Infinite, _NodesMonitoringHeartbeat);
            lock (_nodes)
            {
                foreach (var w in _nodes)
                    w.Active = true;
            }
            _timer.Change(_NodesMonitoringHeartbeat, _NodesMonitoringHeartbeat);
        }

        public void StopManaging()
        {
            _timer.Change(Timeout.Infinite, _NodesMonitoringHeartbeat);
            lock (_nodes)
            {
                foreach (var w in _nodes)
                    w.Active = false;
            }
            _timer.Change(_NodesMonitoringHeartbeat, _NodesMonitoringHeartbeat);
        }

        public void ClearNodes()
        {
            lock (_nodes)
            {
                _nodes.Clear();
            }            
        }

    }
}
