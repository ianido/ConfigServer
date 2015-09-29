using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Xml;

namespace nsteam.ConfigServer.Types
{
    public static class Extensions
    {
        public static string AsString(this XmlDocument xmlDoc)
        {
            using (StringWriter sw = new StringWriter())
            {
                using (XmlTextWriter tx = new XmlTextWriter(sw))
                {
                    xmlDoc.WriteTo(tx);
                    string strXmlText = sw.ToString();
                    return strXmlText;
                }
            }
        }


        public static string RemoveObjectInfo(this string json)
        {
            string objectinfomath = @"(,?\s*"")(objectinfo)(""\s*)(:)(\s*{)";
            Match match = Regex.Match(json, objectinfomath, RegexOptions.IgnoreCase);
            while (match.Success)
            {
                int nbr = 1;
                int i = match.Index + match.Length;
                while (nbr > 0)
                {
                    if (json[i] == '{') nbr++;
                    if (json[i] == '}') nbr--;
                    i++;
                }
                json = json.Remove(match.Index, i - match.Index);
                match = Regex.Match(json, objectinfomath, RegexOptions.IgnoreCase);
            }
            return json;
        }


        public static XmlDocument CreateXMLDocument(dynamic rootobj)
        {
            //string json_target = Json.Encode(rootobj);
            string json_target = JsonConvert.SerializeObject(rootobj);
            var target = json_target.CreateXMLDocument();
            return target;
        }

        public static XmlDocument CreateXMLDocument(this string json)
        {
            json = Regex.Replace(json, @"("")(\w+)(""\s*)(:)(\s*"")", "$1@$2$3$4$5"); // Generate attributes based on single nodes                        
            var target = JsonConvert.DeserializeXmlNode(json.Replace(@"\", @"\\"), null, true);

            return target;
        }

    }
}
