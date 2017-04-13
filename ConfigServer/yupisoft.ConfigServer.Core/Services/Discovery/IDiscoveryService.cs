using System;
using System.Collections.Generic;
using System.Text;

namespace yupisoft.ConfigServer.Core.Services
{
    public interface IServiceDiscovery
    {
        void AttemptStart();
        void AttemptStop();
    }
}
