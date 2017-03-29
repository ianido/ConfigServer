using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace yupisoft.ConfigServer.Core.Cluster
{
    public enum NodeEventType
    {
        RequestClusterSync, // Request Cluster Sync; Get me all the nodes; Initial Message to the Network to Start
        RequestDataSync,    // Request Data Sync;    Get me the data. 
        SendClusterSync,    // Send Cluster Sync to the node.
        SendDataSync,       // Send Data Sync to the node.
        SendDataDiff,       // Send Data Diff to the nodes.
        NotifyDegraded,     // Node performance is degraded.
        SetDisable          // The node is going to be disabled.
    }
    public delegate void NodeEvent(Node sender, NodeEventType type);
    public class ClusterManager
    {
        private Timer _timer;

        private ILogger _logger;
        private ConfigurationChanger _cfgChanger;
        private ConfigServerManager _cfgServer;

        private int _NodesMonitoringHeartbeat = 2000; // Milliseconds
        private int _NodesMonitoringMaxAttempts = 3;
        private int _NodesMonitoringSkipAttemptsOnFail = 3;

        
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
        
        public event NodeEvent Notify;

        protected virtual void OnNodeNotify(Node sender, NodeEventType type)
        {
            if (Notify != null) Notify(sender, type);
        }

        public Node[] GetNodes()
        {
            lock (_nodes)
            {
                return _nodes.ToArray();
            }
        }

        public ClusterManager(IOptions<ClusterConfigSection> clusterConfig, ILogger<ClusterManager> logger, ConfigurationChanger cfgChanger, ConfigServerManager cfgServer)
        {
            _cfgChanger = cfgChanger;
            _cfgServer = cfgServer;
            _logger = logger;
            _nodes = new List<Node>();
            _NodesMonitoringHeartbeat = clusterConfig.Value.NodesMonitoringInterval;
            _NodesMonitoringMaxAttempts = clusterConfig.Value.NodesMonitoringMaxAttempts;
            _NodesMonitoringSkipAttemptsOnFail = clusterConfig.Value.NodesMonitoringSkipAttemptsOnFail;

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
                _logger.LogCritical("The current node is not in the Nodes List.");
                Environment.Exit(1);
            }

            _timer = new Timer(new TimerCallback(Timer_Elapsed), _nodes, Timeout.Infinite, _NodesMonitoringHeartbeat);
            _logger.LogInformation("Created ClusterManager with " + _nodes.Count + " nodes.");
        }

        public HeartBeatMessageResponse ProcessHeartBeat(HeartBeatMessageRequest request)
        {
            UpdateNodes(request.Nodes);
            if (selfNode.InUse)
            {
                HeartBeatMessageResponse response = new HeartBeatMessageResponse();
                response.Created = DateTime.UtcNow;
                response.Command = HeartBeartCommand.InUse;
                _logger.LogTrace("Node in Use; ignoring HeartBeat cmd: " + request.Command.ToString());
                return response;
            }

            if (request.Command == HeartBeartCommand.SyncRequest)
            {
                HeartBeatMessageResponse response = new HeartBeatMessageResponse();
                response.Created = DateTime.UtcNow;
                response.Command = HeartBeartCommand.SyncResponse;
                lock (selfNode)
                {
                    response.Log = selfNode.LogMessages.Where(l => l.LogId > request.LastLogId).ToList();
                    response.LastLogId = selfNode.LastLogId;
                }
            }

            if (request.Command == HeartBeartCommand.HeartBeatRequest)
            {
                HeartBeatMessageResponse response = new HeartBeatMessageResponse();
                response.Created = DateTime.UtcNow;
                response.Command = HeartBeartCommand.HeartBeatResponse;
                lock (selfNode)
                {
                    response.LastLogId = selfNode.LastLogId;
                    response.NodeId = selfNode.Id;
                    if (selfNode.LastLogId < request.LastLogId)
                    {
                        selfNode.Status = SelfNodeStatus.Unsyncronized;
                        HeartBeatMessageRequest req = new HeartBeatMessageRequest();
                        req.Command = HeartBeartCommand.SyncRequest;
                        req.NodeId = selfNode.Id;
                        lock (_nodes) request.Nodes = _nodes.Select(e => e.NodeConfig).ToArray();
                        string msgData = JsonConvert.SerializeObject(request);
                        HttpClient client = new HttpClient();
                        Node requestNode = null;
                        lock (_nodes) { requestNode = _nodes.FirstOrDefault(n => n.Id == request.NodeId); }

                        _logger.LogTrace("Node: " + requestNode.Id + " -> HeartBeat<Sync>");
                        selfNode.InUse = true;
                        requestNode.InUse = true;

                        client.PostAsync(requestNode.Address + "/api/Cluster/HeartBeat", new StringContent(msgData, Encoding.UTF8, "application/json")).ContinueWith((a) =>
                        {
                            lock (selfNode)
                            {
                                _logger.LogTrace("Response from HeartBeat<Sync> for node id: " + requestNode.Id + " Status: " + a.Status);
                                try
                                {
                                    if ((a.Status == TaskStatus.RanToCompletion) && (a.Result.IsSuccessStatusCode))
                                    {
                                        ApiSingleResult<HeartBeatMessageResponse> rsMsg = JsonConvert.DeserializeObject<ApiSingleResult<HeartBeatMessageResponse>>(a.Result.Content.ReadAsStringAsync().Result);
                                        if (rsMsg.Item == null)
                                        {
                                            _logger.LogError("Node: " + requestNode.Id + " do not return valid response. <null>");
                                            return;
                                        }
                                    
                                        if (rsMsg.Item.Command != HeartBeartCommand.SyncResponse)
                                        {
                                            _logger.LogError("Node: " + requestNode.Id + " do not return valid command: " + rsMsg.Item.Command.ToString() + " expected: " + HeartBeartCommand.SyncResponse.ToString());
                                            return;
                                        }

                                        if (rsMsg.Item.Log == null)
                                        {
                                            _logger.LogError("Node: " + requestNode.Id + " do not return valid response <log null>. ");
                                            return;
                                        }

                                        if (rsMsg.Item.Log.Count == 0)
                                        {
                                            _logger.LogError("Node: " + requestNode.Id + " do not return valid response <log empty>. ");
                                            return;
                                        }

                                        var logsToApply = rsMsg.Item.Log.Where(l => l.LogId > selfNode.LastLogId);

                                        foreach (var log in logsToApply)
                                        {
                                            // Apply JsonDiff
                                            bool applied = _cfgServer.ApplyUpdate(log.TenantId, log.Entity, log.JsonDiff);
                                            if (applied) selfNode.LastLogId = log.LogId;
                                        }
                                        _logger.LogInformation("Node: " + requestNode.Id + " HeartBeat<sync> apply " + logsToApply.Count() + "logs successfully.");
                                    }
                                    else
                                    {
                                        _logger.LogError("Unable to contact node: " + requestNode.Id + " for sync. ");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogCritical("Exception procesing node: " + requestNode.Id + ex.ToString());
                                }
                                finally
                                {
                                    requestNode.InUse = false;
                                    selfNode.InUse = false;
                                }
                            }
                        });
                    }
                    // Now Request for syncronization
                }
                return response;

            }

            HeartBeatMessageResponse res = new HeartBeatMessageResponse();
            res.Created = DateTime.UtcNow;
            res.Command = HeartBeartCommand.Unknow;
            _logger.LogTrace("Unknow command: " + request.Command.ToString());
            return res;
        }



        public void UpdateNodes(NodeConfigSection[] nodesConfig)
        {
            if (nodesConfig == null) return;
            Node[] nodes = GetNodes();
            foreach (var node in nodesConfig)
            {
                if (node.Enabled)
                {
                    var foundNode = nodes.FirstOrDefault(e => e.Id == node.Id);
                    if (foundNode != null)
                    {
                        foundNode.Active = true;
                    }
                    else
                    {
                        var newNode = new Node() { Id = node.Id, Active = true, Address = node.Address, Self = false, NodeConfig = node };
                        lock (_nodes)
                        {
                            _logger.LogInformation("Adding New Node Id: " + newNode.Id + ".");
                            _nodes.Add(newNode);
                            _cfgChanger.AddClusterNode(node);
                        }
                    }
                }
            }
        }

        public void HeartBeat(Node node)
        {
            if ((node.Self) || (node.InUse)) return;
            if (node.SkipAttempts > 0)
            {
                node.SkipAttempts--;
                return;
            }
            try
            {   
                // ==========================================================================
                //  Create Request Message here.
                // ==========================================================================

                HeartBeatMessageRequest request = new HeartBeatMessageRequest();
                request.Command = HeartBeartCommand.HeartBeatRequest;
                request.NodeId = selfNode.Id;
                lock (_nodes) request.Nodes = _nodes.Select(e => e.NodeConfig).ToArray();
                string msgData = JsonConvert.SerializeObject(request);
                HttpClient client = new HttpClient();
                _logger.LogTrace("Node: " + node.Id + " heartbeat");
                node.InUse = true;

                client.PostAsync(node.Address + "/api/Cluster/Heartbeat", new StringContent(msgData, Encoding.UTF8, "application/json")).ContinueWith((a) =>
                {
                    lock (node)
                    {
                        _logger.LogTrace("Response from Heartbeat for node id: " + node.Id + " Status: " + a.Status );
                        try
                        {
                            if ((a.Status == TaskStatus.RanToCompletion) && (a.Result.IsSuccessStatusCode))
                            {
                                ApiSingleResult<HeartBeatMessageResponse> rsMsg = JsonConvert.DeserializeObject<ApiSingleResult<HeartBeatMessageResponse>>(a.Result.Content.ReadAsStringAsync().Result);
                                if (rsMsg.Item == null)
                                {
                                    _logger.LogError("Node: " + node.Id + " do not return valid response. attempt: " + node.Attempts);
                                    node.Attempts++;
                                }
                                _logger.LogInformation("Node: " + node.Id + " heartbeat successfully.");
                                node.Attempts = 0;
                                node.SkipAttempts = 0;
                                node.ResetLife();
                                    // ==========================================================================
                                    // TODO : Do Heartbeat operations here.
                                    // ==========================================================================

                                }
                            else
                            {
                                node.Attempts++;
                                _logger.LogError("Unable to contact node: " + node.Id + " attempt: " + node.Attempts);
                            }

                            if (node.Attempts >= _NodesMonitoringMaxAttempts)
                            {
                                if (node.Life == 0)
                                {
                                    lock (_cfgChanger)
                                    {
                                        _cfgChanger.DisableClusterNode(node.Id);
                                        node.Active = false;
                                    }
                                    node.Attempts = 0;
                                    _logger.LogError("Node " + node.Id + " disabled. ");
                                }
                                else
                                {
                                    node.SkipAttempts = _NodesMonitoringSkipAttemptsOnFail;
                                    _logger.LogError("Node " + node.Id + " failed to heartbeat for " + node.Attempts + " attempts; setting node for " + node.SkipAttempts + " skip attemps.");
                                    node.Attempts = 0;
                                    node.Life--;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogCritical("Exception procesing node: " + node.Id + ex.ToString());
                        }
                        finally
                        {
                            node.InUse = false;
                        }
                    }
                });

            }
            catch (Exception ex)
            {
                _logger.LogCritical("Exception procesing node: " + node.Id + ex.ToString());
            }
            finally
            {
                node.InUse = true;
            }

        }

        private void Timer_Elapsed(object state)
        {
            _timer.Change(Timeout.Infinite, _NodesMonitoringHeartbeat); // Disable the timer;
            Node[] nodes = null;
            lock (_nodes)
            {
                nodes = _nodes.ToArray();
            }
            foreach (var w in nodes)
            {
                lock (w)
                {
                    if (w.Active)
                    {
                        HeartBeat(w);
                    }
                }
            }
            _timer.Change(_NodesMonitoringHeartbeat, _NodesMonitoringHeartbeat); // Reenable the timer;
        }

        public void StartManaging()
        {
            _timer.Change(Timeout.Infinite, _NodesMonitoringHeartbeat);
            foreach (var w in _nodes)
                w.Active = true;
            _timer.Change(_NodesMonitoringHeartbeat, _NodesMonitoringHeartbeat);
        }

        public void StopManaging()
        {
            _timer.Change(Timeout.Infinite, _NodesMonitoringHeartbeat);
            foreach (var w in _nodes)
                w.Active = false;
            _timer.Change(_NodesMonitoringHeartbeat, _NodesMonitoringHeartbeat);
        }

        public void ClearNodes()
        {
            _nodes.Clear();
        }

    }
}
