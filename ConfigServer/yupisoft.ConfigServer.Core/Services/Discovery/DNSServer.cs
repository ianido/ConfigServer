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
        public enum DNSQueryClassification
        {
            Unknow,
            Node,
            Nodes,
            Service,
            Address
        }

        private int _defaultNodeTTL = 0;
        private int _defaultServiceTTL = 0;
        private uint _timeStamp = 0; //

        private DNSConfigSection _config;
        private ConfigServerTenants _tenants;
        private ClusterManager _clusterMan;
        private GeoServices _geoServices;

        private ILogger _logger;
        private DnsServer _dnsServer;
        private bool _softStop = true;
        private Dictionary<string,string> _knownAddresses;


        public DNSServer(DNSConfigSection config, ILogger logger, ConfigServerTenants tenants, ClusterManager clusterMan, GeoServices geoServices)
        {
            _config = config;
            _logger = logger;
            _tenants = tenants;
            _clusterMan = clusterMan;
            _geoServices = geoServices;
            _knownAddresses = new Dictionary<string,string>();
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
                    _timeStamp = uint.Parse(DateTime.UtcNow.ToString("MMddHHmmss"));
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

        private Service[] GetService(string serviceName, string tag, string clientAddr)
        {
            List<Service> discoverServices = new List<Service>();
            foreach (var tenant in _tenants.Tenants)
                if (tenant.EnableServiceDiscovery)
                    foreach (var service in tenant.Services)
                        if ((service.Value.Name == serviceName) && (tag == "" || service.Value.Tags.Contains(tag)))
                            discoverServices.Add(service.Value);

            // Reorganize Services base on
            List<Service> returnedServices = new List<Service>();

            foreach (var service in discoverServices)
            {
                ServiceBalancers balancer = service.Balancer;
                if (balancer == ServiceBalancers.GeoLocalization)
                {
                    _geoServices.SortByGeolocation(discoverServices, returnedServices, clientAddr);
                    if (returnedServices.Count == 0) balancer = ServiceBalancers.Random;
                    else
                        break;
                }
                if (balancer == ServiceBalancers.Random)
                {
                    Random rnd = new Random(DateTime.UtcNow.Millisecond);
                    while (discoverServices.Count != returnedServices.Count)
                    {
                        var next = rnd.Next(discoverServices.Count);
                        if (returnedServices.Contains(discoverServices[next])) continue;
                        discoverServices[next].LastChoosed = false;
                        if (returnedServices.Count == 0) discoverServices[next].LastChoosed = true;
                        returnedServices.Add(discoverServices[next]);
                    }
                    break;
                }
                else
                if (balancer == ServiceBalancers.RoundRobin)
                {
                    for (int i = 0; i < discoverServices.Count; i++)
                    {
                        if ((discoverServices[i].LastChoosed.HasValue) && (discoverServices[i].LastChoosed.Value)) {
                            discoverServices[i].LastChoosed = false;
                            continue;
                        }
                        discoverServices[i].LastChoosed = false;
                        if (returnedServices.Count == 0) discoverServices[i].LastChoosed = true;
                        returnedServices.Add(discoverServices[i]);
                    }
                    for (int i = 0; i < discoverServices.Count; i++)
                    {
                        if (returnedServices.Contains(discoverServices[i])) continue;
                        discoverServices[i].LastChoosed = false;
                        returnedServices.Add(discoverServices[i]);
                    }
                    break;
                }
            }            
            return returnedServices.ToArray();
        }

        private Node[] GetAllNodes()
        {
            List<Node> discoverNodes = _clusterMan.Nodes.Where(n => n.Mode == Node.NodeMode.Server && n.Active && n.LastCheckActive).ToList();

            // Reorganize Services base on
            List<Node> returnedNodes = new List<Node>();

            foreach (var node in discoverNodes)
            {
                if (_clusterMan.Balancer == ClusterNodeBalancers.Random)
                {
                    Random rnd = new Random(DateTime.UtcNow.Millisecond);
                    while (discoverNodes.Count != returnedNodes.Count)
                    {
                        var next = rnd.Next(discoverNodes.Count);
                        if (returnedNodes.Contains(discoverNodes[next])) continue;
                        discoverNodes[next].LastChoosed = false;
                        if (returnedNodes.Count == 0) discoverNodes[next].LastChoosed = true;
                        returnedNodes.Add(discoverNodes[next]);
                    }
                    break;
                }
                else
                if (_clusterMan.Balancer == ClusterNodeBalancers.RoundRobin)
                {
                    for (int i = 0; i < discoverNodes.Count; i++)
                    {
                        if ((discoverNodes[i].LastChoosed.HasValue) && (discoverNodes[i].LastChoosed.Value))
                        {
                            discoverNodes[i].LastChoosed = false;
                            continue;
                        }
                        discoverNodes[i].LastChoosed = false;
                        if (returnedNodes.Count == 0) discoverNodes[i].LastChoosed = true;
                        returnedNodes.Add(discoverNodes[i]);
                    }
                    for (int i = 0; i < discoverNodes.Count; i++)
                    {
                        if (returnedNodes.Contains(discoverNodes[i])) continue;
                        discoverNodes[i].LastChoosed = false;
                        returnedNodes.Add(discoverNodes[i]);
                    }
                    break;
                }
            }

            return returnedNodes.ToArray();
        }

        private string IPtoHex(string ip)
        {
            string hex = "";
            try
            {
                foreach (var part in ip.Split('.'))
                    hex += int.Parse(part).ToString("X");
            }
            catch 
            {
                _logger.LogTrace("DNS: Invalid IP Address: " + ip);
            }
            return hex.ToLower();
        }

        private static IPEndPoint Parse(string endpointstring)
        {
            return Parse(endpointstring, -1);
        }

        private static IPEndPoint Parse(string endpointstring, int defaultport)
        {
            if (string.IsNullOrEmpty(endpointstring)
                || endpointstring.Trim().Length == 0)
            {
                throw new ArgumentException("Endpoint descriptor may not be empty.");
            }

            if (defaultport != -1 &&
                (defaultport < IPEndPoint.MinPort
                || defaultport > IPEndPoint.MaxPort))
            {
                throw new ArgumentException(string.Format("Invalid default port '{0}'", defaultport));
            }

            string[] values = endpointstring.Split(new char[] { ':' });
            IPAddress ipaddy;
            int port = -1;

            //check if we have an IPv6 or ports
            if (values.Length <= 2) // ipv4 or hostname
            {
                if (values.Length == 1)
                    //no port is specified, default
                    port = defaultport;
                else
                    port = getPort(values[1]);

                //try to use the address as IPv4, otherwise get hostname
                if (!IPAddress.TryParse(values[0], out ipaddy))
                    ipaddy = getIPfromHost(values[0]).Result;
            }
            else if (values.Length > 2) //ipv6
            {
                //could [a:b:c]:d
                if (values[0].StartsWith("[") && values[values.Length - 2].EndsWith("]"))
                {
                    string ipaddressstring = string.Join(":", values.Take(values.Length - 1).ToArray());
                    ipaddy = IPAddress.Parse(ipaddressstring);
                    port = getPort(values[values.Length - 1]);
                }
                else //[a:b:c] or a:b:c
                {
                    ipaddy = IPAddress.Parse(endpointstring);
                    port = defaultport;
                }
            }
            else
            {
                throw new FormatException(string.Format("Invalid endpoint ipaddress '{0}'", endpointstring));
            }

            if (port == -1)
                throw new ArgumentException(string.Format("No port specified: '{0}'", endpointstring));

            return new IPEndPoint(ipaddy, port);
        }

        private static int getPort(string p)
        {
            int port;

            if (!int.TryParse(p, out port)
             || port < IPEndPoint.MinPort
             || port > IPEndPoint.MaxPort)
            {
                throw new FormatException(string.Format("Invalid end point port '{0}'", p));
            }

            return port;
        }

        private static async Task<IPAddress> getIPfromHost(string p)
        {
            var hosts = await Dns.GetHostAddressesAsync(p);

            if (hosts == null || hosts.Length == 0)
                throw new ArgumentException(string.Format("Host not found: {0}", p));

            return hosts[0];
        }

        private DNSQueryClassification CheckQuery(DomainName domain)
        {
            var parts = domain.Labels;
            var part1 = parts[parts.Length - 1];
            var part2 = (parts.Length > 1) ? parts[parts.Length - 2] : "";
            if ((part1 == "node") || (part2 == "node")) return DNSQueryClassification.Node;
            if ((part1 == "nodes") || (part2 == "nodes")) return DNSQueryClassification.Nodes;
            if ((part1 == "service") || (part2 == "service")) return DNSQueryClassification.Service;
            if ((part1 == "addr") || (part2 == "addr")) return DNSQueryClassification.Address;
            return DNSQueryClassification.Unknow;
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
                string labels = question.Name.ToString();
                _logger.LogTrace("DNS: query: " + labels);

                DnsMessage response = message.CreateResponseInstance();
                response.EDnsOptions = null;

                if (question.Name.Labels.Length < 2)
                {
                    _logger.LogTrace("DNS: Invalid query: " + question.Name.ToString() + "(" + question.RecordType.ToString() + ")");
                    NoResponse(response, eventArgs);
                    return;
                }

                if (question.Name.IsSubDomainOf(DomainName.Parse(_config.Mainzone.Trim('.'))))
                {
                    DomainName domainMainZone = DomainName.Parse(_config.Mainzone.Trim('.'));
                    DomainName workingDomain = new DomainName(question.Name.Labels.Take(question.Name.LabelCount - domainMainZone.LabelCount).ToArray());

                    if ((workingDomain.Labels[workingDomain.LabelCount - 1].ToLower() != "service")
                     && (workingDomain.Labels[workingDomain.LabelCount - 1].ToLower() != "node")
                     && (workingDomain.Labels[workingDomain.LabelCount - 1].ToLower() != "addr")
                     && (workingDomain.Labels[workingDomain.LabelCount - 1].ToLower() != "nodes")
                     && (workingDomain.Labels[workingDomain.LabelCount - 1].ToLower() != _clusterMan.DataCenterId.ToLower()))
                     {
                         _logger.LogTrace("DNS: No this datacenter, not found for query: " + question.Name.ToString() + "(" + question.RecordType.ToString() + ")");
                         NoResponse(response, eventArgs);
                         return;
                     }

                    #region Register all the addresses
                    foreach (var node in _clusterMan.Nodes)
                    {
                        try
                        {
                            lock (_knownAddresses)
                            {
                                if (node.Mode == Node.NodeMode.Client) continue;
                                var lanEndpoint = Parse(new Uri(node.Uri).Authority, 80);
                                var wanEndpoint = Parse(new Uri(node.WANUri).Authority, 80);

                                if (!_knownAddresses.ContainsKey(lanEndpoint.Address.ToString()))
                                    _knownAddresses.Add(lanEndpoint.Address.ToString(), IPtoHex(lanEndpoint.Address.ToString()));
                                if (!_knownAddresses.ContainsKey(wanEndpoint.Address.ToString()))
                                    _knownAddresses.Add(wanEndpoint.Address.ToString(), IPtoHex(wanEndpoint.Address.ToString()));

                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning("DNS: Invalid one of node Uris: " + node.Uri + " or " + node.WANUri + " Error: " + ex.Message);
                        }
                    }

                    foreach (var tenant in _tenants.Tenants)
                        if (tenant.EnableServiceDiscovery)
                            foreach (var service in tenant.Services)
                            {
                                lock (_knownAddresses)
                                {
                                    if (!_knownAddresses.ContainsKey(service.Value.Address))
                                        _knownAddresses.Add(service.Value.Address, IPtoHex(service.Value.Address));
                                }
                            }
                    #endregion

                    #region IF Query Type: .node[.datacenter].<domain>
                    if (CheckQuery(workingDomain) == DNSQueryClassification.Node)
                    {
                        // <node>.node[.datacenter].<domain>
                        // lan.<node>.node[.datacenter].<domain>
                        // wan.<node>.node[.datacenter].<domain>

                        int ind = Array.LastIndexOf(workingDomain.Labels, "node");
                        if (ind == 0)
                        {
                            _logger.LogWarning("DNS: Invalid query, missing node for query: " + question.Name.ToString() + "(" + question.RecordType.ToString() + ")");
                            NoResponse(response, eventArgs);
                            return;
                        }
                        string nodeId = workingDomain.Labels[ind - 1];
                        var node = _clusterMan.Nodes.FirstOrDefault(n => n.Id == nodeId && n.Mode == Node.NodeMode.Server && n.Active && n.LastCheckActive);
                        if (node == null)
                        {
                            _logger.LogWarning("DNS: Node: " + nodeId + " not found for query: " + question.Name.ToString() + "(" + question.RecordType.ToString() + ")");
                            NoResponse(response, eventArgs);
                            return;
                        }
                        // Check DatacenterName

                        var lanEndpoint = Parse(new Uri(node.Uri).Authority, 80);
                        var wanEndpoint = Parse(new Uri(node.WANUri).Authority, 80);
                        var nAddress = lanEndpoint;
                        if (ind - 1 != 0)
                        {
                            // lan.<node>.node[.datacenter].<domain>
                            // wan.<node>.node[.datacenter].<domain>
                            if (!string.IsNullOrEmpty(workingDomain.Labels[ind - 2]))
                            {
                                string wanOrlan = workingDomain.Labels[ind - 2];
                                if (wanOrlan.ToLower() == "wan") nAddress = wanEndpoint;
                            }
                        }
                        if (IPAddress.TryParse(nAddress.Address.ToString(), out IPAddress nodeAddress))
                        {
                            // Check record Type
                            if ((question.RecordType == RecordType.Any) || (question.RecordType == RecordType.A))
                            {
                                response.ReturnCode = ReturnCode.NoError;
                                response.AnswerRecords.Add(new ARecord(question.Name, _defaultNodeTTL, nodeAddress));
                                response.IsAuthoritiveAnswer = true;
                                eventArgs.Response = response;
                                return;
                            }
                            else
                            if ((question.RecordType == RecordType.Srv))
                            {
                                response.ReturnCode = ReturnCode.NoError;
                                if (_knownAddresses.ContainsKey(nAddress.Address.ToString()))
                                {
                                    string target = _knownAddresses[nAddress.Address.ToString()] + ".addr." + _clusterMan.DataCenterId.ToLower() + "." + _config.Mainzone.Trim('.'); ;
                                    response.AnswerRecords.Add(new SrvRecord(question.Name, _defaultServiceTTL, 1, 1, Convert.ToUInt16(nAddress.Port), DomainName.Parse(target)));
                                    response.AdditionalRecords.Add(new ARecord(DomainName.Parse(target), _defaultServiceTTL, nAddress.Address));
                                }
                                else
                                {
                                    _logger.LogTrace("DNS: Can not find Target address for IP: " + nAddress.Address.ToString() + ", for query: " + question.Name.ToString() + "(" + question.RecordType.ToString() + ")");
                                }
                                response.IsAuthoritiveAnswer = true;
                                eventArgs.Response = response;
                                return;
                            }
                            else
                            {
                                _logger.LogTrace("DNS: Invalid Question Type: " + question.RecordType.ToString() + ", for query: " + question.Name.ToString() + "(" + question.RecordType.ToString() + ")");
                                NoResponse(response, eventArgs);
                                return;
                            }
                        }
                        else
                        {
                            _logger.LogTrace("DNS: Invalid IP Address: " + nAddress + ", for query: " + question.Name.ToString() + "(" + question.RecordType.ToString() + ")");
                            NoResponse(response, eventArgs);
                            return;
                        }
                    }
                    #endregion

                    else

                    #region IF Query Type: nodes[.datacenter].<domain>
                    if (CheckQuery(question.Name) == DNSQueryClassification.Nodes)
                    {
                        // nodes[.datacenter].<domain>
                        // lan.nodes[.datacenter].<domain>
                        // wan.nodes[.datacenter].<domain>

                        int ind = Array.LastIndexOf(workingDomain.Labels, "nodes");

                        Node[] nodes = GetAllNodes();

                        foreach (var node in nodes)
                        {
                            var lanEndpoint = Parse(new Uri(node.Uri).Authority, 80);
                            var wanEndpoint = Parse(new Uri(node.WANUri).Authority, 80);
                            var nAddress = lanEndpoint;
                            if (ind != 0)
                            {
                                // lan.<node>.node[.datacenter].<domain>
                                // wan.<node>.node[.datacenter].<domain>
                                if (!string.IsNullOrEmpty(workingDomain.Labels[ind - 1]))
                                {
                                    string wanOrlan = workingDomain.Labels[ind - 1];
                                    if (wanOrlan.ToLower() == "wan") nAddress = wanEndpoint;
                                }
                            }
                            if (IPAddress.TryParse(nAddress.Address.ToString(), out IPAddress nodeAddress))
                            {
                                // Check record Type
                                if ((question.RecordType == RecordType.Any) || (question.RecordType == RecordType.A))
                                {
                                    response.ReturnCode = ReturnCode.NoError;
                                    response.AnswerRecords.Add(new ARecord(question.Name, _defaultNodeTTL, nodeAddress));
                                }
                                else
                                if ((question.RecordType == RecordType.Srv))
                                {
                                    response.ReturnCode = ReturnCode.NoError;
                                    if (_knownAddresses.ContainsKey(nAddress.Address.ToString()))
                                    {
                                        string target = _knownAddresses[nAddress.Address.ToString()] + ".addr." + _clusterMan.DataCenterId.ToLower() + "." + _config.Mainzone.Trim('.'); ;
                                        response.AnswerRecords.Add(new SrvRecord(question.Name, _defaultServiceTTL, 1, 1, Convert.ToUInt16(nAddress.Port), DomainName.Parse(target)));
                                        response.AdditionalRecords.Add(new ARecord(DomainName.Parse(target), _defaultServiceTTL, nAddress.Address));
                                    }
                                    else
                                    {
                                        _logger.LogTrace("DNS: Can not find Target address for IP: " + nAddress.Address.ToString() + ", for query: " + question.Name.ToString() + "(" + question.RecordType.ToString() + ")");
                                    }
                                }
                                else
                                {
                                    _logger.LogTrace("DNS: Invalid Question Type: " + question.RecordType.ToString() + ", for query: " + question.Name.ToString() + "(" + question.RecordType.ToString() + ")");
                                    NoResponse(response, eventArgs);
                                    return;
                                }
                            }
                            else
                            {
                                _logger.LogTrace("DNS: Invalid IP Address: " + nAddress + ", for query: " + question.Name.ToString() + "(" + question.RecordType.ToString() + ")");
                            }
                        }
                        response.IsAuthoritiveAnswer = true;
                        eventArgs.Response = response;
                        return;
                    }
                    #endregion

                    else

                    #region IF Query Type: .service[.datacenter].<domain>
                    if (CheckQuery(workingDomain) == DNSQueryClassification.Service)
                    {

                        int ind = Array.LastIndexOf(workingDomain.Labels, "service");
                        if (ind == 0)
                        {
                            _logger.LogWarning("DNS: Invalid query, missing service for query: " + question.Name.ToString() + "(" + question.RecordType.ToString() + ")");
                            NoResponse(response, eventArgs);
                            return;
                        }

                        // part2.part1.service.datacenter.domain
                        // [tag.]<service>.service[.datacenter].<domain>
                        // _<service>._<tag>.service[.datacenter]<.domain>

                        string serviceName = "";
                        string tag = "";
                        string part1 = workingDomain.Labels[ind - 1];
                        string part2 = "";
                        if (ind - 1 != 0)
                            part2 = workingDomain.Labels[ind - 2];

                        if (part1.StartsWith("_") && part2.StartsWith("_"))
                        {
                            serviceName = part2.Substring(1);
                            tag = part1.Substring(1);
                        }
                        else
                        {
                            serviceName = part1;
                            tag = part2;
                        }

                        var services = GetService(serviceName, tag, eventArgs.RemoteEndpoint.Address.ToString());
                        if ((services == null) || (services.Length == 0))
                        {
                            _logger.LogWarning("DNS: Service: " + serviceName + " not found for query: " + question.Name.ToString() + "(" + question.RecordType.ToString() + ")");
                            NoResponse(response, eventArgs);
                            return;
                        }
                        // Check DatacenterName


                        ushort priority = 1;
                        foreach (var service in services)
                        {
                            if ((service.LastCheckStatus != ServiceCheckStatus.Iddle) && (service.LastCheckStatus != ServiceCheckStatus.Passing))
                                continue;
                            if (IPAddress.TryParse(service.Address, out IPAddress serverAddress))
                            {
                                if ((question.RecordType == RecordType.Any) || (question.RecordType == RecordType.A))
                                {
                                    response.ReturnCode = ReturnCode.NoError;
                                    response.AnswerRecords.Add(new ARecord(question.Name, _defaultServiceTTL, serverAddress));
                                }
                                else if (question.RecordType == RecordType.Srv)
                                {
                                    response.ReturnCode = ReturnCode.NoError;
                                    // Hay que crear una direccion unica para cada ip address
                                    //
                                    // c0a80133.addr.<datacenter>.configserver 
                                    //
                                    if (_knownAddresses.ContainsKey(service.Address))
                                    {
                                        string target = _knownAddresses[service.Address] + ".addr." + _clusterMan.DataCenterId.ToLower() + "." + _config.Mainzone.Trim('.');
                                        response.AnswerRecords.Add(new SrvRecord(question.Name, _defaultServiceTTL, priority, 1, Convert.ToUInt16(service.Port), DomainName.Parse(target)));
                                        response.AdditionalRecords.Add(new ARecord(DomainName.Parse(target), _defaultServiceTTL, serverAddress));
                                    }
                                    else
                                    {
                                        _logger.LogTrace("DNS: Can not find Target address for IP: " + service.Address + ", for query: " + question.Name.ToString() + "(" + question.RecordType.ToString() + ")");
                                    }                                    
                                }
                                else
                                {
                                    _logger.LogWarning("DNS: Invalid Question Type: " + question.RecordType.ToString() + ", for query: " + question.Name.ToString() + "(" + question.RecordType.ToString() + ")");
                                }
                            }
                            else
                            {
                                _logger.LogWarning("DNS: Service: " + serviceName + ", invalid IP Address: " + service.Address + " for query: " + question.Name.ToString() + "(" + question.RecordType.ToString() + ")");
                            }
                            priority++;
                        }
                        response.IsAuthoritiveAnswer = true;
                        eventArgs.Response = response;
                        return;
                    }
                    #endregion

                    else

                    #region IF Query Type: .addr[.datacenter].<domain>
                    if (CheckQuery(workingDomain) == DNSQueryClassification.Address)
                    {
                        // <addr>.addr[.datacenter].<domain>

                        int ind = Array.LastIndexOf(workingDomain.Labels, "addr");
                        if (ind == 0)
                        {
                            _logger.LogWarning("DNS: Invalid query, missing addr for query: " + question.Name.ToString() + "(" + question.RecordType.ToString() + ")");
                            NoResponse(response, eventArgs);
                            return;
                        }
                        string addr = workingDomain.Labels[ind - 1].ToLower();

                        if (!_knownAddresses.ContainsValue(addr))
                        {
                            _logger.LogWarning("DNS: Addr: " + addr + " not found for query: " + question.Name.ToString() + "(" + question.RecordType.ToString() + ")");
                            NoResponse(response, eventArgs);
                            return;
                        }
                        var knownaddr = _knownAddresses.FirstOrDefault(v => v.Value == addr);

                        if (IPAddress.TryParse(knownaddr.Key, out IPAddress kAddress))
                        {
                            // Check record Type
                            if ((question.RecordType == RecordType.Any) || (question.RecordType == RecordType.A))
                            {
                                response.ReturnCode = ReturnCode.NoError;
                                response.AnswerRecords.Add(new ARecord(question.Name, _defaultNodeTTL, kAddress));
                                response.IsAuthoritiveAnswer = true;
                                eventArgs.Response = response;
                                return;
                            }
                            else
                            {
                                _logger.LogWarning("DNS: Invalid Question Type: " + question.RecordType.ToString() + ", for query: " + question.Name.ToString() + "(" + question.RecordType.ToString() + ")");
                                NoResponse(response, eventArgs);
                                return;
                            }
                        }
                        else
                        {
                            _logger.LogError("DNS: Invalid IP Address: " + knownaddr.Key + " (Internal Pool), for query: " + question.Name.ToString() + "(" + question.RecordType.ToString() + ")");
                            NoResponse(response, eventArgs);
                            return;
                        }
                    }
                    #endregion

                    NoResponse(response, eventArgs);
                    return;
                }
                else
                    if (_config.FordwardingEnabled)
                {
                    _logger.LogTrace("DNS: Forwarding for query: " + question.Name.ToString() + "(" + question.RecordType.ToString() + ")");

                    DnsMessage upstreamResponse = await DnsClient.Default.ResolveAsync(question.Name, question.RecordType, question.RecordClass);

                    // if got an answer, copy it to the message sent to the client
                    if (upstreamResponse != null)
                    {
                        foreach (DnsRecordBase record in (upstreamResponse.AnswerRecords))
                            response.AnswerRecords.Add(record);

                        foreach (DnsRecordBase record in (upstreamResponse.AdditionalRecords))
                            response.AdditionalRecords.Add(record);

                        response.ReturnCode = ReturnCode.NoError;
                        response.IsRecursionDesired = true;
                        response.IsAuthoritiveAnswer = false;
                        eventArgs.Response = response;
                    }
                }
            }
        }

        private void NoResponse(DnsMessage response, QueryReceivedEventArgs eventArgs)
        {
            response.ReturnCode = ReturnCode.NxDomain;            
            response.AuthorityRecords.Add(new SoaRecord(DomainName.Parse(_config.Mainzone), 0, DomainName.Parse("ns." + _config.Mainzone), DomainName.Parse("postmaster." + _config.Mainzone), _timeStamp, 3600, 600, 0,0));
            eventArgs.Response = response;
        }

        private void ValidResponse(DnsMessage response, QueryReceivedEventArgs eventArgs)
        {
            response.ReturnCode = ReturnCode.NoError;
        }

        private async Task Server_ClientConnected(object sender, ClientConnectedEventArgs eventArgs)
        {
            
        }
    }
}
