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

    public abstract class ClusterManager : IClusterManager
    {
        protected object _lock = new object();
        protected Timer _heartbeatTimer;
        protected Timer _betweenDatacenterTimer;
        protected ILogger _logger;
        protected ConfigurationChanger _cfgChanger;
        protected DatacenterConfigSection _datacenterConfig;
        protected ClusterConfigSection _clusterConfig;
        protected ConfigServerTenants _tenantManager;
        protected HmacAuthenticationOptions _clusterSecurity;
        protected Random rndGenerator = new Random(DateTime.UtcNow.Millisecond);
        protected long NewLogId
        {
            get
            {
                var dt = new DateTime(2017, 1, 1);
                return Convert.ToInt64((DateTime.UtcNow - dt).TotalMilliseconds);
            }
        }

        protected int _NodesMonitoringRndIntervalHeartbeat { get { return _clusterConfig.Monitoring.RndInterval; } }
        protected int _NodesMonitoringHeartbeat { get { return _clusterConfig.Monitoring.Interval; } }
        protected int _DataCenterMaxIntervalHeartBeat { get { return _clusterConfig.Monitoring.DataCenterMaxInterval; } }
        protected int _DataCenterMinIntervalHeartBeat { get { return _clusterConfig.Monitoring.DataCenterMinInterval; } }
        protected int _NodesMonitoringMaxAttempts { get { return _clusterConfig.Monitoring.MaxAttempts; } }
        protected int _NodesMonitoringSkipAttemptsOnFail { get { return _clusterConfig.Monitoring.SkipAttemptsOnFail; } }
        protected int _NodesMonitoringLife { get { return _clusterConfig.Monitoring.NodesLife; } }

        protected List<Node> _nodes;

        public ClusterNodeBalancers Balancer
        {
            get
            {
                if (_clusterConfig.Balancer.ToLower() == "random")
                    return ClusterNodeBalancers.Random;
                else
                   if (_clusterConfig.Balancer.ToLower() == "roundrobin")
                    return ClusterNodeBalancers.RoundRobin;
                else
                   if (_clusterConfig.Balancer.ToLower().StartsWith("performance"))
                    return ClusterNodeBalancers.Performance;
                // Performance works by performance counters:
                // the config specify the performance counter like: performance:srv_redis01_hitspersec
                // In this case the counter: servicehitspersec will be evaluated and based on the value will determine 
                // which server will choose.
                return ClusterNodeBalancers.Random;
            }
        }
        public string DataCenterId
        {
            get
            {
                return _datacenterConfig.Id;
            }
        }
        public DateTime _AliveSince { get; private set; }

        public Node[] Nodes
        {
            get
            {
                lock (_nodes)
                    return _nodes.ToArray();
            }
        }
        public SelfNode selfNode
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

        public ClusterManager(IOptions<DatacenterConfigSection> datacenterConfig, IOptions<ClusterConfigSection> clusterConfig, ILogger<IClusterManager> logger, ConfigurationChanger cfgChanger, ConfigServerTenants tenantManager)
        {
            _AliveSince = DateTime.UtcNow;
            _cfgChanger = cfgChanger;
            _tenantManager = tenantManager;
            _datacenterConfig = datacenterConfig.Value;
            _clusterConfig = clusterConfig.Value;

            _tenantManager.MergeMode = clusterConfig.Value.MergeMode;

            foreach (var tenant in tenantManager.Tenants)
                tenant.Store.Change += Store_Change;

            _logger = logger;
            _nodes = new List<Node>();
            _clusterSecurity = _clusterConfig.Security;
            var nodesConfig = _clusterConfig.Nodes;
            foreach(var node in nodesConfig)
            {
                if (string.IsNullOrEmpty(node.DataCenter)) node.DataCenter = DataCenterId;
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
                var newNode = new SelfNode(new NodeConfigSection() { Id = clusterConfig.Value.OwnNodeName, WANUri = clusterConfig.Value.OwnNodeUrl, Uri = clusterConfig.Value.OwnNodeUrl, Enabled = true, Mode = "server", DataCenter = DataCenterId, HeartBeat = true  });
                _nodes.Add(newNode);
            }

            if (selfNode == null)
            {
                _logger.LogCritical("The current node (SelfNode) is not in the Nodes List.");
                Environment.Exit(1);
            }

            _betweenDatacenterTimer = new Timer(new TimerCallback(DataCenterTimer_Elapsed), _nodes, Timeout.Infinite, _DataCenterMaxIntervalHeartBeat);
            _heartbeatTimer = new Timer(new TimerCallback(Timer_Elapsed), _nodes, Timeout.Infinite, _NodesMonitoringHeartbeat);
            _logger.LogInformation("Created ClusterManager with " + _nodes.Count + " nodes.");
        }
        public void StartManaging()
        {
            StartNodeHeartBeat();
            _betweenDatacenterTimer.Change(rndGenerator.Next(_DataCenterMaxIntervalHeartBeat) + _DataCenterMinIntervalHeartBeat, rndGenerator.Next(_DataCenterMaxIntervalHeartBeat) + _DataCenterMinIntervalHeartBeat);
        }
        public void StopManaging()
        {
            StopNodeHeartBeat();
            _betweenDatacenterTimer.Change(rndGenerator.Next(_DataCenterMaxIntervalHeartBeat) + _DataCenterMinIntervalHeartBeat, rndGenerator.Next(_DataCenterMaxIntervalHeartBeat) + _DataCenterMinIntervalHeartBeat);
        }
        public void StopNodeHeartBeat()
        {
            _heartbeatTimer.Change(Timeout.Infinite, _NodesMonitoringHeartbeat);
        }
        public void StartNodeHeartBeat()
        {
            _heartbeatTimer.Change(rndGenerator.Next(_NodesMonitoringRndIntervalHeartbeat) + _NodesMonitoringHeartbeat, _NodesMonitoringHeartbeat);
        }


        protected abstract void Timer_Elapsed(object state);

        protected abstract void DataCenterTimer_Elapsed(object state);

        protected abstract void Store_Change(ConfigServerTenant tenant, IStoreProvider sender, string entityName);

        public abstract HeartBeatMessage ReceiveHeartBeat(HeartBeatMessage msg);
    }
}
