using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp6
{
    public class TestMergeDiff
    {
        public static void Method1()
        {
            JObject t1 = JObject.Parse("{ 'a':'1', 'b':'2', 'c':'1' }");
            JObject t2 = JObject.Parse("{ 'a':'1', 'b':'3', 'c':'3' }");
            JObject t3 = JObject.Parse("{ 'a':'1', 'b':'2', 'c':'3' }");
            var jsonDiff = new JsonDiffPatchDotNet.JsonDiffPatch();
            JObject d1 = (JObject)jsonDiff.Diff(t1, t2);
            Console.WriteLine("D1");
            Console.WriteLine(d1.ToString());
            JObject d2 = (JObject)jsonDiff.Diff(t2, t3);
            Console.WriteLine("D2");
            Console.WriteLine(d2.ToString());
            d1.Merge(d2, new JsonMergeSettings() {  MergeArrayHandling = MergeArrayHandling.Merge});
            Console.WriteLine("D1 merged");
            Console.WriteLine(d1.ToString());
        }
    }
}
