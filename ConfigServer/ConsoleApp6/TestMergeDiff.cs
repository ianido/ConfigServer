using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApp6
{
    public class TestMergeDiff
    {
        public static string NormalizeJsonString(JObject parsedObject)
        {

            // Sort properties of JObject.
            var normalizedObject = SortPropertiesAlphabetically(parsedObject);

            // Serialize JObject .
            return JsonConvert.SerializeObject(normalizedObject);
        }

        private static JObject SortPropertiesAlphabetically(JObject original)
        {
            var result = new JObject();

            foreach (var property in original.Properties().ToList().OrderBy(p => p.Name))
            {
                var value = property.Value as JObject;

                if (value != null)
                {
                    value = SortPropertiesAlphabetically(value);
                    result.Add(property.Name, value);
                }
                else
                {
                    result.Add(property.Name, property.Value);
                }
            }

            return result;
        }

        public static void Method1()
        {
            JObject t1 = JObject.Parse("{ 'a':'1', 'b':'2', 'c':'1', d:[1,2,3] }");
            JObject t2 = JObject.Parse("{ 'a':'1', 'b':'2', 'c':'1', d:[1,2,3,4] }");
            JObject t3 = JObject.Parse("{ 'a':'1', 'b':'2', 'c':'1', d:[1,2,3,5] }");
            var jsonDiff = new JsonDiffPatchDotNet.JsonDiffPatch(new JsonDiffPatchDotNet.Options() { TextDiff = JsonDiffPatchDotNet.TextDiffMode.Simple, ArrayDiff = JsonDiffPatchDotNet.ArrayDiffMode.Simple });

            JObject d1 = (JObject)jsonDiff.Diff(t1, t2);
            Console.WriteLine("D1");
            Console.WriteLine(d1.ToString());

            JObject d2 = (JObject)jsonDiff.Diff(t1, t3);
            Console.WriteLine("D2");
            Console.WriteLine(d2.ToString());


            t1 = (JObject)jsonDiff.Patch(t1, d1);
            t1 = (JObject)jsonDiff.Patch(t1, d2);
            Console.WriteLine("T1 applied");
            Console.WriteLine(t1.ToString());
        }
    }
}
