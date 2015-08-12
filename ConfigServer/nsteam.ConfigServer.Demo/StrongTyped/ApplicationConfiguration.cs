using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace nsteam.ConfigServer.Demo
{
    public class ApplicationConfiguration
    {
        public class ConnectionStringType
        {
            public string Name { get; set; }
            public string ConnectionString { get; set; }
            public string ProviderName { get; set; }
        }

        public class ServiceModelType
        {
            public class ServiceModelClient
            {
                public List<ServiceModelClientEndPoint> endpoint { get; set; }
            }

            public class ServiceModelClientEndPoint
            {
                public string Name { get; set; }
                public string Address { get; set; }
            }

            public ServiceModelClient Client { get; set; }

        }

        public string Name { get; set; }
        public int Port { get; set; }
        public ConnectionStringType ConnectionString { get; set; }
        public ServiceModelType ServiceModel { get; set; }

    }
}
