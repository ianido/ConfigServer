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

    public class RaftClusterManager : ClusterManager
    {

        public RaftClusterManager(IOptions<DatacenterConfigSection> datacenterConfig, IOptions<ClusterConfigSection> clusterConfig, ILogger<IClusterManager> logger, ConfigurationChanger cfgChanger, ConfigServerTenants tenantManager) :
            base(datacenterConfig, clusterConfig, logger, cfgChanger, tenantManager)
        {

        }

        protected override void Timer_Elapsed(object state)
        {
            _heartbeatTimer.Change(Timeout.Infinite, _NodesMonitoringHeartbeat); // Disable the timer;

            _heartbeatTimer.Change(rndGenerator.Next(_NodesMonitoringRndIntervalHeartbeat) + _NodesMonitoringHeartbeat, _NodesMonitoringHeartbeat); // Reenable the timer;
        }
        protected override void DataCenterTimer_Elapsed(object state)
        {
            _betweenDatacenterTimer.Change(Timeout.Infinite, _DataCenterMaxIntervalHeartBeat); // Disable the timer;

            _betweenDatacenterTimer.Change(rndGenerator.Next(_DataCenterMaxIntervalHeartBeat) + _DataCenterMinIntervalHeartBeat, rndGenerator.Next(_DataCenterMaxIntervalHeartBeat) + _DataCenterMinIntervalHeartBeat);
        }
        protected override void Store_Change(ConfigServerTenant tenant, IStoreProvider sender, string entityName)
        {

        }
        public override HeartBeatMessage ReceiveHeartBeat(HeartBeatMessage msg)
        {
            return null;
        }

    }
}
