using System;
using System.Collections.Generic;
using System.Text;
using yupisoft.ConfigServer.Core.Utils;

namespace yupisoft.ConfigServer.Core.Hooks
{
    public class HookCheckResult
    {
        public string CheckerId { get; set; }
        public HookCheckStatus Result  { get; set; }
    }
}
