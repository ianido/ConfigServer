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

                _lastCheckStatus = ServiceCheckStatus.InProgress;
                // Determine the result.
                if (CheckResults.Exists(e => e.Result == ServiceCheckStatus.Passing))
                    _lastCheckStatus = ServiceCheckStatus.Passing;
                if (CheckResults.Exists(e => e.Result == ServiceCheckStatus.Warning))
                    _lastCheckStatus = ServiceCheckStatus.Warning;
                if (CheckResults.Exists(e => e.Result == ServiceCheckStatus.Failing))
                    _lastCheckStatus = ServiceCheckStatus.Failing;

                res.Result = ServiceCheckStatus.Iddle;
            }
        }

        private void Sc_CheckStarted(string checkerId)
        {
           var checkResult = CheckResults.FirstOrDefault(c => c.CheckerId == checkerId);
           if (checkResult != null) checkResult.Result = ServiceCheckStatus.InProgress;
        }

        public ServiceCheckStatus LastCheckStatus { get => _lastCheckStatus; }
        public string Id { get { return Config.Id; } }
        public string Name { get { return Config.Name; } }
        public string Address { get { return Config.Address; } }
        public int Port { get { return Config.Port; } }
        public string[] Tags { get { return Config.Tags; } }
        public JServiceConfig Config { get; private set; }
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
            List<ServiceCheck> lchecks = new List<ServiceCheck>();
            CheckResults = new List<ServiceCheckResult>();
            foreach (var check in config.Checks)
            {
                if (check.Disabled) continue;
                var sc = ServiceCheck.CreateFromConfig(check, config, logger);
                sc.CheckDone += Sc_CheckDone;
                sc.CheckStarted += Sc_CheckStarted;
                lchecks.Add(sc);
                ServiceCheckResult res = new ServiceCheckResult();
                res.Result = ServiceCheckStatus.Iddle;
                res.CheckerId = check.Id;
                CheckResults.Add(res);
            }
            Checks = lchecks.ToArray();           
        }


    }
}
