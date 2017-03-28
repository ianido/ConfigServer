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

        private int _NodesMonitoringHeartbeat = 2000; // Milliseconds
        private int _NodesMonitoringMaxAttempts = 3;
        private int _NodesMonitoringSkipAttemptsOnFail = 3;

        List<Node> _nodes;        

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

        public ClusterManager(IOptions<ClusterConfigSection> clusterConfig, ILogger<ClusterManager> logger, ConfigurationChanger cfgChanger)
        {
            _cfgChanger = cfgChanger;
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
                    var newNode = new Node() { Id = node.Id, Active = true, Address = node.Address, Self = (clusterConfig.Value.OwnNodeName == (node.Id)), NodeConfig = node };
                    _nodes.Add(newNode);
                }
            }

            _timer = new Timer(new TimerCallback(Timer_Elapsed), _nodes, Timeout.Infinite, _NodesMonitoringHeartbeat);
            _logger.LogInformation("Created ClusterManager with " + _nodes.Count + " nodes.");
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
                            _nodes.Add(newNode);
                            _cfgChanger.AddClusterNode(node);
                        }
                    }
                }
            }
        }

        public void HeartBeat(Node node)
        {
            lock (node)
            {   
                if ((node.Self) || (node.InUse)) return;
                if (node.SkipAttempts > 0)
                {
                    node.SkipAttempts--;
                    return;
                }                
                HeartBeatMessageRequest request = new HeartBeatMessageRequest();
                // ==========================================================================
                // TODO : Create Request Message here.
                // ==========================================================================
                request.Nodes = _nodes.Select(e=>e.NodeConfig).ToArray();

                string msgData = JsonConvert.SerializeObject(request);
                HttpClient client = new HttpClient();
                //client.Timeout = TimeSpan.FromSeconds(2);
                _logger.LogTrace("Node: " + node.Id + " heartbeat");
                node.InUse = true;

                client.PostAsync(node.Address + "/api/Cluster/Heartbeat", new StringContent(msgData, Encoding.UTF8, "application/json")).ContinueWith((a) =>
                {
                    lock (node)
                    {
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
                            if ((a.Status == TaskStatus.Faulted) || (a.Status == TaskStatus.Canceled))
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
                        finally
                        {
                            node.InUse = false;
                        }
                    }
                });
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
                if (w.Active)
                {
                    HeartBeat(w);                    
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

        public void AddNode(Node node)
        {
            
        }

        public void ClearNodes()
        {
            _nodes.Clear();
        }

    }
}
