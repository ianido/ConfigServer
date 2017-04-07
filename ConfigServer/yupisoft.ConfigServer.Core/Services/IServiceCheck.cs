using System;
using System.Collections.Generic;
using System.Text;

namespace yupisoft.ConfigServer.Core.Services
{
    public interface IServiceCheck
    {
        ServiceCheckStatus LastCheckStatus { get; }
        void Check(int callid);
    }
}
