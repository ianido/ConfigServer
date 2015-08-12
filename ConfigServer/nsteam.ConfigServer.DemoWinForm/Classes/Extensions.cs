using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace nsteam.ConfigServer.DemoWinForm
{

    class JsonHelper
    {
        private const string INDENT_STRING = "    ";
        public static string FormatJson(string str)
        {
            var indent = 0;
            var quoted = false;
            var sb = new StringBuilder();
            for (var i = 0; i < str.Length; i++)
            {
                var ch = str[i];
                switch (ch)
                {
                    case '{':
                    case '[':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            Enumerable.Range(0, ++indent).ForEach(item => sb.Append(INDENT_STRING));
                        }
                        break;
                    case '}':
                    case ']':
                        if (!quoted)
                        {
                            sb.AppendLine();
                            Enumerable.Range(0, --indent).ForEach(item => sb.Append(INDENT_STRING));
                        }
                        sb.Append(ch);
                        break;
                    case '"':
                        sb.Append(ch);
                        bool escaped = false;
                        var index = i;
                        while (index > 0 && str[--index] == '\\')
                            escaped = !escaped;
                        if (!escaped)
                            quoted = !quoted;
                        break;
                    case ',':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            Enumerable.Range(0, indent).ForEach(item => sb.Append(INDENT_STRING));
                        }
                        break;
                    case ':':
                        sb.Append(ch);
                        if (!quoted)
                            sb.Append(" ");
                        break;
                    default:
                        sb.Append(ch);
                        break;
                }
            }
            return sb.ToString();
        }
        public static string RemoveObjectInfo(string json)
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
    }

    static class Extensions
    {
        public static void ForEach<T>(this IEnumerable<T> ie, Action<T> action)
        {
            foreach (var i in ie)
            {
                action(i);
            }
        }
    }

}
