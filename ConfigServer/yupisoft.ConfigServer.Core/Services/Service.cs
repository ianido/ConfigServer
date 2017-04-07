using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;

namespace yupisoft.ConfigServer.Core.Services
{
    public delegate void AllCheckDoneEventHandler(Service service, ServiceCheckStatus status);
    public class Service
    {
        private object _sync1 = new object();
        private object _sync2 = new object();
        private ServiceCheckStatus _lastCheckStatus;

        private void Sc_CheckDone(string CheckerId, ServiceCheckStatus status, int callid)
        {
            ServiceCheckResult res = new ServiceCheckResult();
            res.CallId = callid;
            res.Result = status;
            res.CheckerId = CheckerId;
            lock (_sync1)
            {
                CheckResults.Add(res);
                if (CheckResults.Count(e => e.CallId == callid) == Checks.Length)
                {
                    lock (_sync2)
                    {
                        _lastCheckStatus = ServiceCheckStatus.Nocheck;
                        // Determine the result.
                        if (CheckResults.Exists(e => e.CallId == callid && e.Result == ServiceCheckStatus.Passing))
                            _lastCheckStatus = ServiceCheckStatus.Passing;
                        if (CheckResults.Exists(e => e.CallId == callid && e.Result == ServiceCheckStatus.Warning))
                            _lastCheckStatus = ServiceCheckStatus.Warning;
                        if (CheckResults.Exists(e => e.CallId == callid && e.Result == ServiceCheckStatus.Failing))
                            _lastCheckStatus = ServiceCheckStatus.Failing;
                        OnAllChecksDone(this, _lastCheckStatus);
                    }
                }
            }
        }

        protected virtual void OnAllChecksDone(Service service, ServiceCheckStatus status)
        {
            AllChecksDone?.Invoke(service, status);
        }

        public event AllCheckDoneEventHandler AllChecksDone;

        public ServiceCheckStatus LastCheckStatus { get => _lastCheckStatus; }
        public string Id { get { return Config.Id; } }
        public string Name { get { return Config.Name; } }
        public string Address { get { return Config.Address; } }
        public int Port { get { return Config.Port; } }
        public string[] Tags { get { return Config.Tags; } }
        public JServiceConfig Config { get; private set; }
        public ServiceCheck[] Checks { get; set; }
        public List<ServiceCheckResult> CheckResults { get; set; }

        public int Check()
        {
            Random rnd = new Random(DateTime.Now.Millisecond);
            int callid = rnd.Next(100000);
            foreach (var check in Checks)
            {
                check.Check(callid);
            }
            return callid;
        }
        public Service(JServiceConfig config)
        {
            Config = config;
            List<ServiceCheck> lchecks = new List<ServiceCheck>();
            foreach (var check in config.Checks)
            {
                var sc = ServiceCheck.CreateFromConfig(check, config);
                sc.CheckDone += Sc_CheckDone;
                lchecks.Add(sc);
            }
            Checks = lchecks.ToArray();
            CheckResults = new List<ServiceCheckResult>();
        }

    }
}
