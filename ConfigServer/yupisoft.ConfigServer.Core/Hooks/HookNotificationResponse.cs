using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using yupisoft.ConfigServer.Core.Utils;

namespace yupisoft.ConfigServer.Core.Hooks
{
    public class HookNotificationResponse
    {
        public string NotificationId { get; set; }
        public string Data { get; set; }
        public HookNotificationResult Result  { get; set; }
    }
}
