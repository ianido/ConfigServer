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

        private int HEARTBEAT_MILLESECONDS = 2000;

        List<Node> _nodes;        

        public event NodeEvent Notify;

        protected virtual void OnNodeNotify(Node sender, NodeEventType type)
        {
            if (Notify != null) Notify(sender, type);
        }

        public ClusterManager(IOptions<ClusterConfigSection> clusterConfig, ILogger<ClusterManager> logger)
        {
            _logger = logger;
            _nodes = new List<Node>();
            var nodesConfig = clusterConfig.Value.Nodes;
            foreach(var node in nodesConfig)
            {
                if (node.Enabled)
                    _nodes.Add(new Node() { Id = node.Name, Active = true, Address = node.Address });
            }

            _timer = new Timer(new TimerCallback(Timer_Elapsed), _nodes, Timeout.Infinite, HEARTBEAT_MILLESECONDS);
        }

        public void HeartBeat(Node node)
        {
            HeartBeatMessageRequest request = new HeartBeatMessageRequest();
            string msgData = JsonConvert.SerializeObject(request);
            HttpClient client = new HttpClient();
            client.PostAsync(node.Address + "/api/Cluster/Heartbeat", new StringContent(msgData, Encoding.UTF8)).ContinueWith((a) => {
                if (a.Result.IsSuccessStatusCode)
                {
                    ApiSingleResult<HeartBeatMessageResponse> rsMsg = JsonConvert.DeserializeObject<ApiSingleResult<HeartBeatMessageResponse>>(a.Result.Content.ReadAsStringAsync().Result);
                    node.Active = (rsMsg.Item != null);
                }
            });
        }

        private void Timer_Elapsed(object state)
        {
            _timer.Change(Timeout.Infinite, HEARTBEAT_MILLESECONDS); // Disable the timer;
            foreach (var w in _nodes.ToList())
            {
                HeartBeat(w);
            }
            _timer.Change(HEARTBEAT_MILLESECONDS, HEARTBEAT_MILLESECONDS); // Reenable the timer;
        }

        public void StartMonitoring()
        {
            _timer.Change(Timeout.Infinite, HEARTBEAT_MILLESECONDS);
            foreach (var w in _nodes)
                w.Active = true;
            _timer.Change(HEARTBEAT_MILLESECONDS, HEARTBEAT_MILLESECONDS);
        }

        public void StopMonitoring()
        {
            _timer.Change(Timeout.Infinite, HEARTBEAT_MILLESECONDS);
            foreach (var w in _nodes)
                w.Active = false;
            _timer.Change(HEARTBEAT_MILLESECONDS, HEARTBEAT_MILLESECONDS);
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
