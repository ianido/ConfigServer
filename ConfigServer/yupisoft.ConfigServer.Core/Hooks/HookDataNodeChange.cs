using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using yupisoft.ConfigServer.Core.Utils;

namespace yupisoft.ConfigServer.Core.Hooks
{
    public class HookDataNodeChange : Hook
    {
        public string Node { get { return Config.Id; } }
        public string Http { get { return Config.Http; } }
        public string HttpMethod { get { return Config.HttpMethod; } }

        public HookDataNodeChange(JHookConfig config, ILogger logger) : base(config, logger)
        {
        }

        protected override Task<HookCheckStatus> CheckAsync(){
            return null;
        } 
    }
}
