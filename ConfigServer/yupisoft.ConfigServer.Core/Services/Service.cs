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

        public string Id { get { return Config.Id; } }
        public string Name { get { return Config.Name; } }
        public string Address { get { return Config.Address; } }
        public int Port { get { return Config.Port; } }
        public string[] Tags { get { return Config.Tags; } }

        protected ServiceCheckStatus _lastCheckStatus;
        protected virtual void OnAllChecksDone(Service service, ServiceCheckStatus status)
        {
            AllChecksDone?.Invoke(service, status);
        }
        public JServiceConfig Config { get; private set; }
        public ServiceCheck[] Checks { get; set; }
        public List<ServiceCheckResult> CheckResults { get; set; }
        public event AllCheckDoneEventHandler AllChecksDone;
        public int Check()
        {
            Random rnd = new Random(DateTime.Now.Millisecond);
            int callid = rnd.Next(100000);
            foreach (var check in Checks)
            {
                Task task = new Task((a) => check.Check(callid), check).WithTimeout(check.Timeout);
                task.ContinueWith((t) =>
                {
                    // Saber si fue timeout o no. de acuerdo a esto hay que ponerle un valor al check status
                    // ver que pasa con la otra tarea si la tarea de timeout retorna, (se cancela?) 
                });
            }
            return callid;
        }
        public Service(JServiceConfig config)
        {
            Config = config;
            List<ServiceCheck> lchecks = new List<ServiceCheck>();
            foreach (var check in config.Checks)
            {
                var sc = ServiceCheck.CreateFromConfig(check);
                sc.CheckDone += Sc_CheckDone;
                lchecks.Add(sc);
            }
            Checks = lchecks.ToArray();
            CheckResults = new List<ServiceCheckResult>();
        }
        private void Sc_CheckDone(string CheckerId, ServiceCheckStatus status, int callid)
        {
            ServiceCheckResult res = new ServiceCheckResult();
            res.CallId = callid;
            res.Result = status;
            res.CheckerId = CheckerId;
            lock (_sync1) {
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
    }
}
