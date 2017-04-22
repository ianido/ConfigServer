using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using yupisoft.ConfigServer.Core.Utils;

namespace yupisoft.ConfigServer.Core.Hooks
{
    public class HookCheckResult : IHookCheckResult
    {
        public Hook Hook { get; set; }
        public JToken Data { get; set; }
        public HookCheckStatus Result  { get; set; }
        public HookCheckResult()
        {
            Result = HookCheckStatus.Iddle;
        }
    }
}
