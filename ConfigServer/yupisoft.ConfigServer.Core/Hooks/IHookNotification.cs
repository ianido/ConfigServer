using System;
using System.Collections.Generic;
using System.Text;

namespace yupisoft.ConfigServer.Core.Hooks
{
    
    public interface IHookNotification
    {
        void Notify(IHookCheckResult checkResults);
    }
}
