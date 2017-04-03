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
        public static string SignMessage(SignedMessage message, string secret)
        {
            message.Signature = secret;
            string serializedMessage = JsonConvert.SerializeObject(message, Formatting.None);
            string csignature = CalculateHash(serializedMessage).ToString();
            message.Signature = csignature;
            return JsonConvert.SerializeObject(message, Formatting.None);
        }

        public static bool CheckMessageSignature(SignedMessage message, string secret)
        {
            string signature = message.Signature;
            message.Signature = secret;
            string serializedMessage = JsonConvert.SerializeObject(message, Formatting.None);
            string csignature = CalculateHash(serializedMessage).ToString();
            return (csignature == signature);
        }

    }
}
