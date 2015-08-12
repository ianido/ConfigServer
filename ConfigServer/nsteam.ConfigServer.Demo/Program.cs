using nsteam.ConfigServer.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;

namespace nsteam.ConfigServer.Demo
{
    class Program
    {

        static ConfigService srv = null;

        static void Main(string[] args)
        {

            string basenode = "configuration";
            string getnode1 = "enviroments[@Test]";
            string getnode2 = "enviroments[@Production]";
            string getnode3 = "enviroments[@Test].applications[@LogServer]";
            
            /*
            string basenode = "configuration";
            string getnode = "newobject";
            */
            
            Console.WriteLine("Connectiong to the service...");
            srv = new ConfigService("http://localhost:9000/", basenode);
            Console.WriteLine("====== P R E S S   E N T E R   T O   S T A R T   D E M O =======");
            Console.ReadLine();

            Test_Concurrency(100);

            Test_SetNode(getnode3);

            Test_GetTree();

            Test_GetTree(getnode1);

            Test_GetTree(getnode2);

            Test_GetTree_Secuential(getnode2, 1000);

            Test_GetTree_Parallel(getnode2, 1000);

            Test_GetTree(getnode3);            

            Test_GetTree(getnode3);
            
        }

        private static void Test_GetTree()
        {
            dynamic obj = srv.GetTree();
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);

            Console.WriteLine("GetTree()");
            Console.ReadLine();
            Console.WriteLine(json);
            Console.WriteLine("===========================");
            Console.WriteLine("Press enter to continue... ");
            Console.ReadLine();
        }

        private static void Test_GetTree(string getnode)
        {
            dynamic obj = srv.GetTree(getnode);
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);

            Console.WriteLine("GetTree(" + getnode + ")");
            Console.WriteLine("===========================");
            Console.WriteLine(json);
            Console.WriteLine("===========================");

            Console.WriteLine("Press enter to continue... ");
            Console.ReadLine();
        }

        private static void Test_GetTree_Secuential(string getnode, int p)
        {
            Console.WriteLine("PERFORMANCE TEST READING " + p + " TIMES SECUENTIAL " + getnode);
            Console.WriteLine("Press enter to continue... ");
            Console.ReadLine();

            Stopwatch tsw = new Stopwatch();
            int totalbytes = 0;
            tsw.Start();
            for (int i = 0; i < p; i++)
            {
                dynamic obj1a = srv.GetTree(getnode);
                string json1a = Newtonsoft.Json.JsonConvert.SerializeObject(obj1a, Newtonsoft.Json.Formatting.Indented);
                totalbytes += json1a.Length;
                Console.Write(".");
            }
            tsw.Stop();
            Console.WriteLine("=======================================================================");
            Console.WriteLine("TOTAL TIME:" + tsw.Elapsed.TotalSeconds + "s ===> " + totalbytes + " bytes.");
            Console.WriteLine(tsw.Elapsed.TotalSeconds / 1000 + "s per request ");
            Console.WriteLine("Press enter to continue... ");
            Console.ReadLine();
        }

        private static void Test_GetTree_Parallel(string getnode, int p)
        {
            Console.WriteLine("PERFORMANCE TEST READING " + p + " TIMES PARALLEL " + getnode);
            Console.WriteLine("Press enter to continue... ");
            Console.ReadLine();

            Stopwatch tsw = new Stopwatch();
            int totalbytes = 0;
            tsw.Start();

            Parallel.For(0, p, new ParallelOptions { MaxDegreeOfParallelism = 4 }, k =>
            {
                dynamic obj1a = srv.GetTree(getnode);
                string json1a = Newtonsoft.Json.JsonConvert.SerializeObject(obj1a, Newtonsoft.Json.Formatting.Indented);
                totalbytes += json1a.Length;
                Console.Write(".");
            });

            tsw.Stop();
            Console.WriteLine("=======================================================================");
            Console.WriteLine("TOTAL TIME:" + tsw.Elapsed.TotalSeconds + "s ===> " + totalbytes + " bytes.");
            Console.WriteLine(tsw.Elapsed.TotalSeconds / 1000 + "s per request ");
            Console.WriteLine("Press enter to continue... ");
            Console.ReadLine();
        }

        private static void Test_GetNode(string getnode)
        {
            dynamic obj = srv.GetNode(getnode);
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);

            Console.WriteLine("GetNode(" + getnode + ")");
            Console.WriteLine("===========================");
            Console.WriteLine(json);
            Console.WriteLine("===========================");
            Console.ReadLine();

        }

        private static void Test_SetNode(string getnode)
        {
            TNode node = srv.GetNode(getnode);
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(node, Newtonsoft.Json.Formatting.Indented);

            Console.WriteLine("GetNode(" + getnode + ")");
            Console.WriteLine("===========================");
            Console.WriteLine(json);
            Console.WriteLine("===========================");
            Console.ReadLine();
            Console.WriteLine("===========================");
            Console.WriteLine("node.Value.address = \"SQLSERVER\"");
            Console.WriteLine("===========================");
            node.Value.address = "SQLSERVER";
            srv.SaveNode(node);
            Console.WriteLine("Press enter to continue... ");
            Console.ReadLine();
        }

        private static void Test_Concurrency(int p)
        {
            string getnode = "enviroments[@Test].applications[@LogServer]";
            Console.WriteLine("CONCURRENCY TEST READING AND WRITING " + p + " TIMES PARALLEL ");
            Console.WriteLine("Press enter to continue... ");
            Console.ReadLine();

            Stopwatch tsw = new Stopwatch();
            int totalbytes = 0;
            tsw.Start();

            Console.WriteLine("Getting node: " + getnode);
            TNode node = srv.GetNode(getnode);
            node.Value.address = "1";
            Console.WriteLine("Set Address to 1");
            srv.SaveNode(node);

            Console.WriteLine("Press enter to continue... ");
            Console.ReadLine();

            Parallel.For(0, p, new ParallelOptions { MaxDegreeOfParallelism = 4 }, k =>
            {
                node = srv.GetNode(getnode);
                node.Value.address = (int.Parse((string)node.Value.address) + 1).ToString();
                srv.SaveNode(node);
                Console.Write(".");
            });           
            tsw.Stop();

            node = srv.GetNode(getnode);
            Console.WriteLine("");
            Console.WriteLine("Final value of Address: " + node.Value.address);

            Console.WriteLine("=======================================================================");
            Console.WriteLine("TOTAL TIME:" + tsw.Elapsed.TotalSeconds + "s ===> " + totalbytes + " bytes.");
            Console.WriteLine(tsw.Elapsed.TotalSeconds / 1000 + "s per request ");
            Console.ReadLine();
        }
        
    }
}
