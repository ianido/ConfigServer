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
                        newNode = new SelfNode(node);
                    else
                        newNode = new Node(node);

                    _nodes.Add(newNode);
                }
            }

            if (selfNode == null)
            {
                var newNode = new SelfNode(new NodeConfigSection() { Id = clusterConfig.Value.OwnNodeName, Address = clusterConfig.Value.OwnNodeUrl, Enabled = true }) ;
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

        private HeartBeatMessage CreateMessageFor(HeartBeartType type, HeartBeatMessage msg)
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
                
                if (msg.LastLogId < selfNode.LastLogId)
                {
                    var logMessages = selfNode.LogMessages.Where(m => m.LogId > msg.LastLogId);
                    message.Log.AddRange(logMessages);
                    message.LastLogId = selfNode.LastLogId;
                    message.LastLogDate = selfNode.LastLogDate;
                }
                else
                    if ( DifferentDataHash &&
                           ( 
                            (selfNode.LastLogId == msg.LastLogId) 
                             && ((message.NodeAliveSince < msg.NodeAliveSince) || (selfNode.LastLogDate > msg.LastLogDate))
                           )
                       )
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

                if (
                      (
                        (selfNode.LastLogId == msg.LastLogId) &&
                        ((_cfgServer.AliveSince < msg.NodeAliveSince) || (selfNode.LastLogDate > msg.LastLogDate))
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
            else
            if (msg.MessageType == HeartBeartType.UpdateRequest)
            {
                node.Attempts = 0;
                node.SkipAttempts = 0;
                node.ResetLife();
                _cfgServer.ApplyUpdate(msg.Log, selfNode.LogMessages);
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

                    selfNode.InUse = true;
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
                                        _logger.LogTrace(rsMsg.Item.MessageType + " " + selfNode.Id + " Log(" + selfNode.LastLogId + ") <-- " + node.Id + " Log(" + rsMsg.Item.LastLogId + ")");
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
                                _logger.LogCritical("Exception heartbeating node: " + node.Id + "\r\n" + ex.ToString());
                            }
                            finally
                            {
                                node.InUse = false;
                                selfNode.InUse = false;

                                #region Check for max lives
                                try
                                {
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
                    selfNode.InUse = false;
                    node.InUse = false;
                    _logger.LogCritical("Exception processing node: " + node.Id + ex.ToString());
                }
            }
        }

        public HeartBeatMessage ReceiveHeartBeat(HeartBeatMessage msg)
        {
            if (selfNode.InUse)
            {
                _logger.LogTrace(msg.MessageType + " " + selfNode.Id + "(InUse) Log(" + selfNode.LastLogId + ") <-- " + msg.NodeId + " Log(" + msg.LastLogId + ")");
                var response = CreateMessageFor(HeartBeartType.HeartBeatResponse, msg);
                return response;
            }
            _logger.LogTrace(msg.MessageType + " " + selfNode.Id + " Log(" + selfNode.LastLogId + ") <-- " + msg.NodeId + " Log(" + msg.LastLogId + ")");
            if (msg.MessageType == HeartBeartType.UpdateRequest)
            {
                _cfgServer.ApplyUpdate(msg.Log, selfNode.LogMessages);

                var response = CreateMessageFor(HeartBeartType.UpdateResponse, msg);
                return response;
            }
            else
            if (msg.MessageType == HeartBeartType.HeartBeatRequest)
            {
                if (
                     (
                       (selfNode.LastLogId == msg.LastLogId) && 
                       ((_cfgServer.AliveSince < msg.NodeAliveSince) || (selfNode.LastLogDate > msg.LastLogDate))
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

        private void _cfgServer_DataChanged(string tenantId, string entity, JToken diffToken)
        {
            _logger.LogInformation("Data changed Tenant: " + tenantId + " Entity: " + entity);
            _logger.LogTrace("Diff:" + diffToken?.ToString());

            LogMessage lm = new LogMessage() {Created = DateTime.UtcNow, Entity = entity, TenantId = tenantId, JsonDiff = diffToken?.ToString(Formatting.None) };
            lock (selfNode)
            {                
                lm.LogId = (selfNode.LogMessages.Count > 0) ? (selfNode.LogMessages.Last().LogId + 1) : 1;
                selfNode.LogMessages.Add(lm);
            }
        }

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
