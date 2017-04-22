using System;
using System.Collections.Generic;
using System.Text;

namespace yupisoft.ConfigServer.Core.Services
{
    public enum ServiceCheckStatus
    {
        Nocheck,
        Iddle,
        InProgress,
        Passing,
        Warning,
        Failing
    }
}
