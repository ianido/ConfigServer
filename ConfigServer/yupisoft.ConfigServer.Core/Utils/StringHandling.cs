using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using yupisoft.ConfigServer.Core.Cluster;

namespace yupisoft.ConfigServer.Core.Utils
{
    public static class StringHandling
    {
        public static UInt64 CalculateHash(string read)
        {
            UInt64 hashedValue = 3074457345618258791ul;
            for (int i = 0; i < read.Length; i++)
            {
                hashedValue += read[i];
                hashedValue *= 3074457345618258799ul;
            }
            return hashedValue;
        }
    }
}
