using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace yupisoft.ConfigServer.Core.Services
{
    public class Service
    {
        private object _sync1 = new object();

        private ServiceCheckStatus _lastCheckStatus;
        private ILogger _logger;

        private void Sc_CheckDone(string CheckerId, ServiceCheckStatus status)
        {
            lock (_sync1)
            {
                _logger.LogTrace("Srv: " + this.Id + " Chk: " + CheckerId + " -> " + status.ToString());
                ServiceCheckResult res = CheckResults.FirstOrDefault(c => c.CheckerId == CheckerId);
                res.Result = status;
                ServiceCheckStatus lstatus = ServiceCheckStatus.InProgress;
                // Determine the result.
                if (CheckResults.Exists(e => e.Result == ServiceCheckStatus.Passing))
                    lstatus = ServiceCheckStatus.Passing;
                if (CheckResults.Exists(e => e.Result == ServiceCheckStatus.Warning))
                    lstatus = ServiceCheckStatus.Warning;
                if (CheckResults.Exists(e => e.Result == ServiceCheckStatus.Failing))
                    lstatus = ServiceCheckStatus.Failing;
                _lastCheckStatus = lstatus;
                res.Result = ServiceCheckStatus.Iddle;
            }
        }

        private void Sc_CheckStarted(string checkerId)
        {
           var checkResult = CheckResults.FirstOrDefault(c => c.CheckerId == checkerId);
           if (checkResult != null) checkResult.Result = ServiceCheckStatus.InProgress;
        }

        public bool? LastChoosed { get; set; } // Determine if this server was selected by the balancer.
        public ServiceCheckStatus LastCheckStatus { get => _lastCheckStatus; }
        public string Id { get { return Config.Id; } }
        public string Name { get { return Config.Name; } }
        public ServiceBalancers Balancer {
            get {
                if (Config.Balancer.ToLower() == "random")
                    return ServiceBalancers.Random;
                else
                if (Config.Balancer.ToLower() == "roundrobin")
                    return ServiceBalancers.RoundRobin;
                else
                if (Config.Balancer.ToLower().StartsWith("performance"))
                    return ServiceBalancers.Performance;
                else
                if (Config.Balancer.ToLower().StartsWith("geo"))
                    return ServiceBalancers.GeoLocalization;
                // Performance works by performance counters:
                // the config specify the performance counter like: performance:srv_redis01_hitspersec
                // In this case the counter: servicehitspersec will be evaluated and based on the value will determine 
                // which server will choose.
                return ServiceBalancers.Random;
            }
        }
        public string Address { get { return Config.Address; } }
        public int Port { get { return Config.Port; } }
        public string[] Tags { get { return Config.Tags; } }
        public JServiceConfig Config { get; private set; }
        public JServiceGeoConfig Geo {
            get
            {
                foreach (var t in Tags)
                {
                    if (t.StartsWith("geo-"))
                    {
                        string[] parts = t.Split('-');
                        if (parts.Length == 3)
                        {
                            var geo = new JServiceGeoConfig();
                            if (parts[1].ToLower() == "countries") geo.Countries = parts[2];
                            if (parts[1].ToLower() == "continents") geo.Continents = parts[2];
                            if (parts[1].ToLower() == "regions") geo.Regions = parts[2];
                            if (parts[1].ToLower() == "pos") geo.GeoPos = parts[2]; // lat,long
                            return geo;
                        } else
                            if (parts.Length == 2)
                            {
                                var geo = new JServiceGeoConfig();
                                if (parts[1].ToLower() == "pos") geo.GeoPos = "client"; // client location
                                return geo;
                            }
                    }
                }
                return null;
            }
        }
        public ServiceCheck[] Checks { get; set; }
        public List<ServiceCheckResult> CheckResults { get; set; }

        private Random rnd = new Random(DateTime.Now.Millisecond);

        public void Check()
        {
            lock (_sync1)
            {                
                foreach (var check in Checks)
                {
                    var cresult = CheckResults.FirstOrDefault(c => c.CheckerId == check.Id);
                    if (cresult != null && cresult.Result == ServiceCheckStatus.Iddle) check.Check();
                }
            }
        }
        public Service(JServiceConfig config, ILogger logger)
        {
            _logger = logger;
            Config = config;
            LastChoosed = null;
            _lastCheckStatus = ServiceCheckStatus.Iddle;
            List<ServiceCheck> lchecks = new List<ServiceCheck>();
            CheckResults = new List<ServiceCheckResult>();
            foreach (var check in config.Checks)
            {
                if (check.Disabled) continue;
                var sc = ServiceCheck.CreateFromConfig(check, config, logger);
                if (sc != null)
                {
                    sc.CheckDone += Sc_CheckDone;
                    sc.CheckStarted += Sc_CheckStarted;
                    lchecks.Add(sc);
                    ServiceCheckResult res = new ServiceCheckResult();
                    res.Result = ServiceCheckStatus.Iddle;
                    res.CheckerId = check.Id;
                    CheckResults.Add(res);
                }
            }
            Checks = lchecks.ToArray();           
        }


    }
}
