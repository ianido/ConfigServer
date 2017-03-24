using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public ClusterManager(ILogger<IConfigWatcher> logger)
        {
            _logger = logger;
            _nodes = new List<Node>();
            _timer = new Timer(new TimerCallback(Timer_Elapsed), _nodes, Timeout.Infinite, HEARTBEAT_MILLESECONDS);
        }

        private void Timer_Elapsed(object state)
        {
            //Check for file modifications
            _timer.Change(Timeout.Infinite, HEARTBEAT_MILLESECONDS); // Disable the timer;

                foreach (var w in _nodes.ToList())
                {
                    w.Enabled = true;
                }

            _timer.Change(HEARTBEAT_MILLESECONDS, HEARTBEAT_MILLESECONDS); // Reenable the timer;
        }

        public void StartMonitoring()
        {
            _timer.Change(Timeout.Infinite, HEARTBEAT_MILLESECONDS);
            foreach (var w in _nodes)
                w.Enabled = true;
            _timer.Change(HEARTBEAT_MILLESECONDS, HEARTBEAT_MILLESECONDS);
        }

        public void StopMonitoring()
        {
            _timer.Change(Timeout.Infinite, HEARTBEAT_MILLESECONDS);
            foreach (var w in _nodes)
                w.Enabled = false;
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
