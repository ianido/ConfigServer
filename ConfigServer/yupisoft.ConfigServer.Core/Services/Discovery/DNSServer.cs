using ARSoft.Tools.Net;
using ARSoft.Tools.Net.Dns;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using yupisoft.ConfigServer.Core.Cluster;

namespace yupisoft.ConfigServer.Core.Services
{
    public class DNSServer : IServiceDiscovery
    {
        private DNSConfigSection _config;
        private ConfigServerTenants _tenants;
        private ClusterManager _clusterMan;
        private ILogger _logger;
        private DnsServer _dnsServer;
        private bool _softStop = true;
        private Dictionary<string, Service> _serviceAddresses;


        public DNSServer(DNSConfigSection config, ILogger logger, ConfigServerTenants tenants, ClusterManager clusterMan)
        {
            _config = config;
            _logger = logger;
            _tenants = tenants;
            _clusterMan = clusterMan;
            _serviceAddresses = new Dictionary<string, Service>();
            try
            {
                IPAddress addr = IPAddress.Any;
                if (_config.BindAddress.ToLower() != "any") addr = IPAddress.Parse(_config.BindAddress);
                IPEndPoint endpoint = new IPEndPoint(addr, _config.Port);
                _dnsServer = new DnsServer(endpoint, _config.udpListenerCount, _config.tcpListenerCount);
                _dnsServer.ClientConnected += Server_ClientConnected;
                _dnsServer.QueryReceived += Server_QueryReceived;
                _logger.LogInformation("Starting DNS Server on Port:" + _config.Port + " Masterzone: " + _config.Mainzone + " Bound to Interface :" + _config.BindAddress);
                _dnsServer.Start();
            }
            catch (Exception ex)
            {
                _logger.LogError("Unable to start DNS Server: " + ex.Message);
            }
        }

        public void AttemptStart()
        {
            if (_config.Enabled)
            {
                try
                {
                    _softStop = false;
                    // _dnsServer.Start();
                    _logger.LogTrace("Starting DNS Server Queries.");
                }
                catch (Exception ex)
                {
                    _logger.LogError("Unable to start DNS Server: " + ex.Message);
                }
            }
            else
                _logger.LogInformation("DNS Server is disabled");
        }

        public void AttemptStop()
        {
            if (_dnsServer != null)
            {
                try
                {
                    _softStop = true;
                    //_dnsServer.Stop(); // For some reason this stop do not work.
                    //_logger.LogInformation("Stopping DNS Server on Port:" + _config.Port + ".");
                    _logger.LogTrace("Pausing DNS Server Queries.");
                }
                catch(Exception ex)
                {
                    _logger.LogError("Unable to stop DNS Server: " + ex.Message);
                }
            }
        }

        private Service GetService(string serviceName, string tag)
        {
            foreach (var tenant in _tenants.Tenants)
                if (tenant.EnableServiceDiscovery)
                    foreach (var service in tenant.Services)
                        if ((service.Value.Name == serviceName) && (service.Value.Tags.Contains(tag)))
                            return service.Value;
            return null;
        }


        private async Task Server_QueryReceived(object sender, QueryReceivedEventArgs eventArgs)
        {
            if (_softStop) return;

            DnsMessage message = eventArgs.Query as DnsMessage;
            if (message == null)
                return;


            if ((message.Questions.Count == 1))
            {
                // send query to upstream server
                DnsQuestion question = message.Questions[0];
                DnsMessage response = message.CreateResponseInstance();

                if (question.Name.Labels.Length < 3)
                {
                    _logger.LogTrace("DNS: Invalid query: " + string.Join(".", question.Name.Labels));
                    NoResponse(response, eventArgs);
                    return;
                }

                if (question.Name.Labels[question.Name.Labels.Length - 1] == _config.Mainzone.Trim('.'))
                {                    
                    List<Service> foundServices = new List<Service>();

                    if (question.Name.Labels.Contains("node"))
                    {
                        // <node>.node[.datacenter].<domain>
                        // lan.<node>.node[.datacenter].<domain>
                        // wan.<node>.node[.datacenter].<domain>

                        int ind = Array.IndexOf(question.Name.Labels, "node");
                        if (ind == 0) {
                            _logger.LogWarning("DNS: Invalid query, missing node for query: " + string.Join(".",question.Name.Labels));
                            NoResponse(response, eventArgs);
                            return;
                        }
                        string nodeId = question.Name.Labels[ind - 1];
                        var node = _clusterMan.Nodes.FirstOrDefault(n => n.Id == nodeId);
                        if (node == null)
                        {
                            _logger.LogWarning("DNS: Node: " + nodeId + " not found for query: " + string.Join(".", question.Name.Labels));
                            NoResponse(response, eventArgs);
                            return;
                        }
                        // Check DatacenterName
                        if (question.Name.Labels[question.Name.Labels.Length - 2] != _clusterMan.DataCenterId)
                        {
                            _logger.LogTrace("DNS: No this datacenter, node: " + nodeId + " not found for query: " + string.Join(".", question.Name.Labels));
                            NoResponse(response, eventArgs);
                            return;
                        }
                        string nAddress = node.Address;
                        if (ind - 1 != 0)
                        {
                            // lan.<node>.node[.datacenter].<domain>
                            // wan.<node>.node[.datacenter].<domain>
                            if (!string.IsNullOrEmpty(question.Name.Labels[ind - 2]))
                            {
                                string wanOrlan = question.Name.Labels[ind - 2];
                                if (wanOrlan.ToLower() == "wan") nAddress = node.WANAddress;
                            }
                        }
                        if (IPAddress.TryParse(nAddress, out IPAddress nodeAddress))
                        {
                            // Check record Type
                            if ((question.RecordType == RecordType.Any) || (question.RecordType == RecordType.A))
                            {
                                response.ReturnCode = ReturnCode.NoError;
                                response.AnswerRecords.Add(new ARecord(question.Name, 3600, nodeAddress));
                                return;
                            }
                            else
                            {
                                _logger.LogTrace("DNS: Invalid Record Type: " + question.RecordType.ToString() + ", for query: " + string.Join(".", question.Name.Labels));
                                NoResponse(response, eventArgs);
                            }
                        }
                        else
                        {
                            // If Node Address is not IP; Attempt to resolve the address using local DNS
                            DnsMessage upstreamResponse = await DnsClient.Default.ResolveAsync(DomainName.Parse(nAddress), RecordType.A, RecordClass.Any);
                            if ((upstreamResponse != null) && (upstreamResponse.AnswerRecords.Count > 0) && (upstreamResponse.AnswerRecords[0].RecordType == RecordType.A))
                            {
                                nodeAddress = ((ARecord)upstreamResponse.AnswerRecords[0]).Address;
                                response.ReturnCode = ReturnCode.NoError;
                                response.AnswerRecords.Add(new ARecord(question.Name, 3600, nodeAddress));
                                return;
                            }
                        }
                    }
                     else 


                    if (question.Name.Labels.Contains("service"))
                    {
                        int ind = Array.IndexOf(question.Name.Labels, "service");
                        if (ind == 0)
                        {
                            _logger.LogWarning("DNS: Invalid query, missing service for query: " + string.Join(".", question.Name.Labels));
                            NoResponse(response, eventArgs);
                            return;
                        }

                        // part2.part1.service.datacenter.domain
                        // [tag.]<service>.service[.datacenter].<domain>
                        // _<service>._<tag>.service[.datacenter]<.domain>

                        string serviceName = "";
                        string tag = "";
                        string part1 = question.Name.Labels[ind - 1];
                        string part2 = "";
                        if (ind - 1 != 0)
                            part2 = question.Name.Labels[ind - 2];

                        if (part1.StartsWith("_") && part2.StartsWith("_"))
                        {
                            serviceName = part2;
                            tag = part1;
                        } else
                        {
                            serviceName = part1;
                            tag = part2;
                        }

                        var service = GetService(serviceName, tag);
                        if (service == null)
                        {
                            _logger.LogWarning("DNS: Service: " + serviceName + " not found for query: " + string.Join(".", question.Name.Labels));
                            NoResponse(response, eventArgs);
                            return;
                        }
                        // Check DatacenterName
                        if (question.Name.Labels[question.Name.Labels.Length - 2] != _clusterMan.DataCenterId)
                        {
                            _logger.LogTrace("DNS: No this datacenter, service: " + serviceName + " not found for query: " + string.Join(".", question.Name.Labels));
                            NoResponse(response, eventArgs);
                            return;
                        }
                        if (IPAddress.TryParse(service.Address, out IPAddress nodeAddress))
                        {
                            if ((question.RecordType == RecordType.Any) || (question.RecordType == RecordType.A))
                            {
                                response.ReturnCode = ReturnCode.NoError;
                                response.AnswerRecords.Add(new ARecord(question.Name, 3600, nodeAddress));
                                return;
                            }
                            else if (question.RecordType == RecordType.Srv)
                            {
                                response.ReturnCode = ReturnCode.NoError;
                                // Hay que crear una direccion unica para cada servicio/nodo/datacenter/puerto
                                // Se puede calcular un hash con estos valores y asi obtener un unico address. 
                                //
                                // c0a80133.addr.<datacenter>.configserver 
                                //

                                return;
                            }
                            else
                            {
                                _logger.LogTrace("DNS: Invalid Record Type: " + question.RecordType.ToString() + ", for query: " + string.Join(".", question.Name.Labels));
                                NoResponse(response, eventArgs);
                            }
                        }
                        else
                        {
                            _logger.LogWarning("DNS: Service: " + serviceName + ", invalid IP Address: " + service.Address + " for query: " + string.Join(".", question.Name.Labels));
                            NoResponse(response, eventArgs);
                            return;
                        }
                    }

                    
                }
                else
                    if (_config.FordwardingEnabled)
                    {
                        DnsMessage upstreamResponse = await DnsClient.Default.ResolveAsync(question.Name, question.RecordType, question.RecordClass);

                        // if got an answer, copy it to the message sent to the client
                        if (upstreamResponse != null)
                        {
                            foreach (DnsRecordBase record in (upstreamResponse.AnswerRecords))
                                response.AnswerRecords.Add(record);

                            foreach (DnsRecordBase record in (upstreamResponse.AdditionalRecords))
                                response.AdditionalRecords.Add(record);

                            response.ReturnCode = ReturnCode.NoError;
                            eventArgs.Response = response;
                        }

                    }
            }
        }

        private void NoResponse(DnsMessage response, QueryReceivedEventArgs eventArgs)
        {
            response.ReturnCode = ReturnCode.NoError;
        }

        private void ValidResponse(DnsMessage response, QueryReceivedEventArgs eventArgs)
        {
            response.ReturnCode = ReturnCode.NoError;
        }


        private async Task Server_ClientConnected(object sender, ClientConnectedEventArgs eventArgs)
        {
            return;
        }
    }
}
