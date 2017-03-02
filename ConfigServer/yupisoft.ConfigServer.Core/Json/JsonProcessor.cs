using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using yupisoft.ConfigServer.Core.Stores;

namespace yupisoft.ConfigServer.Core.Json
{
    public class JsonProcessor
    {
        private static JToken CreateFromContent(object content)
        {
            JToken token = content as JToken;
            if (token != null)
            {
                return token;
            }
            return new JValue(content);
        }
        private static void MergeEnumerableContent(JContainer target, IEnumerable content, JsonMergeSettings settings, JTokenEqualityComparer comparer)
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
        private static void Merge(JContainer target, object content, JsonMergeSettings settings)
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

        private static bool Nav(JToken tree, JToken token, string entityName, IStoreProvider storeProvider)
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
                    else if (((JProperty)token.Parent).Name == "$include")
                    {
                        JToken refToken = storeProvider.Get(value.Substring(1));
                        if (refToken != null)
                        {
                            token.Replace(refToken);
                            token = refToken;
                        }
                    }
                    else
                    {
                        // This is a reference.
                        JToken refToken = tree.SelectToken(value.Substring(1));
                        if (refToken == null) // There is no node referenced
                        {
                            // Analize the path
                            // Example: archive.json.base.node1
                            var followingParts = value.Substring(1).Split('.');
                            string filename = "";
                            for (int i = 0; i < followingParts.Length; i++) {
                                filename += ((filename.Length != 0) ? "." : "") + followingParts[i];
                                if ((refToken = storeProvider.Get(filename)) != null)
                                {
                                    // Check for more parts?
                                    if (i < followingParts.Length - 1)
                                    {
                                        string jpath = string.Join(".", followingParts, i + 1, followingParts.Length - i - 1);
                                        refToken = refToken.SelectToken(jpath);
                                    }
                                    break;
                                }
                            }                            
                        }
                        if (refToken != null)
                        {
                            token.Replace(refToken);
                            token = refToken;
                        }
                        result = true;
                    }
                }
            }

            foreach (var o in token.ToArray())
            {
                if (o is JToken) result = result || Nav(tree, o, entityName, storeProvider);
            }
            return result;
        }

        private static void Process(JToken token, string entityName, IStoreProvider storeProvider)
        {
            while (Nav(token, token, entityName, storeProvider)) { };
        }

        public static JToken Process(string content, string entityName, IStoreProvider storeProvider)
        {
            var obj = JsonConvert.DeserializeObject<JToken>(content);
            Process(obj, entityName, storeProvider);
            
            return obj;            
        }

    }
}
