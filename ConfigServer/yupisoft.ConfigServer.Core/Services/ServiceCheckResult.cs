﻿using System;
using System.Collections.Generic;
using System.Text;
using yupisoft.ConfigServer.Core.Utils;

namespace yupisoft.ConfigServer.Core.Services
{
    public class ServiceCheckResult
    {
        public string CheckerId { get; set; }
        public ServiceCheckStatus Result  { get; set; }
    }
}
