using Newtonsoft.Json.Linq;
using System.Dynamic;

namespace yupisoft.ConfigServer.Core.Services
{
    public class Service
    {
        public JServiceConfig Config { get; private set; }
        public ServiceCheck[] Checks { get; set; }

        /*
        public ServiceCheckStatus Check()
        {
            foreach(var check in Checks)
            {
                if (check.LastCheckStatus == ServiceCheckStatus.Failing)
                    return 
            }
        }
        */
        public Service(JServiceConfig config)
        {
            Config = config;

            //foreach ()
        }

    }
}
