﻿using System;
using System.Collections.Generic;
using System.Text;

namespace yupisoft.ConfigServer.Core.Cluster
{
    public class LogMessage
    {
        public int LogId { get; set; }
        public int TenantId { get; set; }
        public string Entity { get; set; }
        public string JsonDiff { get; set; }
    }
}
