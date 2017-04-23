namespace yupisoft.ConfigServer.Core.Cluster
{
    public interface IClusterManager
    {
        ClusterNodeBalancers Balancer { get; }
        string DataCenterId { get; }
        Node[] Nodes { get; }
        SelfNode selfNode { get; }
        HeartBeatMessage ReceiveHeartBeat(HeartBeatMessage msg);
        void StartManaging();
        void StopManaging();
    }
}