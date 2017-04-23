using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace yupisoft.ConfigServer.Core.Utils
{
    public static class JSonExtensions
    {
        public static JToken Ordered(this JToken original)
        {
            if (!(original is JObject)) return original;
            var result = new JObject();
            JObject orig = (JObject)original;
            foreach (var property in orig.Properties().ToList().OrderBy(p => p.Name))
            {
                var value = property.Value as JObject;
                if (value != null)
                {
                    value = (JObject)Ordered(value);
                    result.Add(property.Name, value);
                }
                else
                {
                    result.Add(property.Name, property.Value);
                }
            }
            return result;
        }



        public static ulong Hash(this JToken original)
        {
            JToken obj = null;
            if (original is JObject) obj = Ordered((JObject)original);
            string serializedJson = obj.ToString(Newtonsoft.Json.Formatting.None);
            ulong dataHash = StringHandling.CalculateHash(serializedJson);
            return dataHash;
        }
    }
}
