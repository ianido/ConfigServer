using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace yupisoft.ConfigServer.Core.Json
{
    public class JsonProcessor
    {
        internal static JToken CreateFromContent(object content)
        {
            JToken token = content as JToken;
            if (token != null)
            {
                return token;
            }
            return new JValue(content);
        }
        internal static void MergeEnumerableContent(JContainer target, IEnumerable content, JsonMergeSettings settings, JTokenEqualityComparer comparer)
        {
            switch (settings.MergeArrayHandling)
            {
                case MergeArrayHandling.Concat:
                    foreach (JToken item in content)
                    {
                        target.Add(item);
                    }
                    break;
                case MergeArrayHandling.Union:
                    HashSet<JToken> items = new HashSet<JToken>(target, comparer);
                    foreach (JToken item in content)
                    {
                        if (items.Add(item))
                        {
                            target.Add(item);
                        }
                    }
                    break;
                case MergeArrayHandling.Replace:
                    ClassUtils.Invoke("ClearItems", target, null);
                    //target.ClearItems();
                    foreach (JToken item in content)
                    {
                        target.Add(item);
                    }
                    break;
                case MergeArrayHandling.Merge:
                    int i = 0;
                    foreach (object targetItem in content)
                    {
                        if (((JObject)targetItem)["$key"] != null)
                        {
                            string key = ((JObject)targetItem)["$key"].Value<string>();
                            JToken sourceItem = target.SelectToken("[?(@.$key=='" + key + "')]");

                            JContainer existingContainer = sourceItem as JContainer;
                            if (existingContainer != null)
                            {
                                existingContainer.Merge(targetItem, settings);
                            }
                            else
                            {
                                if (targetItem != null)
                                {
                                    JToken contentValue = CreateFromContent(targetItem);
                                    if (contentValue.Type != JTokenType.Null)
                                    {
                                        target.Add(contentValue);
                                    }
                                }
                            }
                            continue;
                        }


                        if (i < target.Count)
                        {
                            JToken sourceItem = target[i];

                            JContainer existingContainer = sourceItem as JContainer;
                            if (existingContainer != null)
                            {
                                existingContainer.Merge(targetItem, settings);
                            }
                            else
                            {
                                if (targetItem != null)
                                {
                                    JToken contentValue = CreateFromContent(targetItem);
                                    if (contentValue.Type != JTokenType.Null)
                                    {
                                        target[i] = contentValue;
                                    }
                                }
                            }
                        }
                        else
                        {
                            target.Add(targetItem);
                        }

                        i++;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(settings), "Unexpected merge array handling when merging JSON.");
            }
        }
        internal static void Merge(JContainer target, object content, JsonMergeSettings settings)
        {
            switch (target.GetType().ToString())
            {
                case "JArray":
                    {
                        bool isMultiContent = ClassUtils.Invoke<bool>("IsMultiContent", target, content);
                        IEnumerable a = (isMultiContent || content is JArray) ? (IEnumerable)content : null;
                        if (a == null)
                        {
                            return;
                        }

                        MergeEnumerableContent(target, a, settings, new JTokenEqualityComparer());
                    }
                    break;
                default:
                    {
                        ClassUtils.Invoke("MergeItem", target, content, settings);
                    }
                    break;
            }
        }
        internal static bool Nav(JToken tree, JToken token)
        {
            bool result = false;
            if (token.Type == JTokenType.String)
            {
                string value = token.Value<string>() ?? "";
                if (value.StartsWith("*"))
                {
                    if (((JProperty)token.Parent).Name == "$inherits")
                    {
                        JToken refToken = tree.SelectToken(value.Substring(1));
                        if (!(refToken is JContainer)) throw new Exception("Inheritance should be from an object.");
                        JToken clonedRefToken = refToken.DeepClone();
                        var toReplace = token.Parent.Parent; // Save the parent
                        token.Parent.Remove(); // Remove inherits field
                        Merge(((JContainer)clonedRefToken), toReplace, new JsonMergeSettings() { MergeArrayHandling = MergeArrayHandling.Merge });
                        toReplace.Replace(clonedRefToken);
                        result = true;
                    }
                    else
                    {
                        if (value.Contains("\\"))
                        {
                            // Is a file


                        }
                        // This is a reference.
                        JToken refToken = tree.SelectToken(value.Substring(1));
                        token.Replace(refToken);
                        result = true;
                    }
                }
            }

            foreach (var o in token.ToArray())
            {
                if (o is JToken) result = result || Nav(tree, o);
            }
            return result;
        }

        public static void Process()
        {
            string baseLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string content = System.IO.File.ReadAllText(Path.Combine(baseLocation, "../../../Examples/fullcomplexinheritance.json"));
            var obj = JsonConvert.DeserializeObject<JToken>(content);

            while (Nav(obj, obj)) { };
            Console.WriteLine(obj.ToString());
        }
    }
}
