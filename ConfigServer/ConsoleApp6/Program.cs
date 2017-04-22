using Newtonsoft.Json;
using System;
using yupisoft.ConfigServer.Client;

namespace ConsoleApp6
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press Enter to start");
            Console.ReadLine();
            //TestAsync.RunMethod();
            TestMergeDiff.Method1();
            Console.WriteLine("Press Enter to exit");
            Console.ReadLine();

            /*

            string text = "";
            do
            {
                Console.WriteLine("Creating client...");
                //ConfigService cfg = new ConfigService("http://localhost:27764", "",1);
                ConfigService cfg = new ConfigService("http://localhost:8000", "", 1);
                Console.WriteLine("Requesting Path: Configuration");
                dynamic obj = cfg.Get<dynamic>("configuration");
                string content = JsonConvert.SerializeObject(obj, Formatting.Indented);

                Console.WriteLine(content);
                Console.WriteLine("=============================");
                Console.WriteLine("Enter 'X' to Exit, Y to modify an element");
                text = Console.ReadLine();

                if (text == "Y")
                {
                    var node = cfg.Get<TNode<dynamic>>("configuration");
                    node.Value.baseobject.sub1 = "sucio";
                    cfg.Set(node);
                    Console.WriteLine("... MODIFIED ...");
                    Console.WriteLine("=============================");
                    Console.WriteLine("Enter 'X' to Exit, Y to modify an element");
                    text = Console.ReadLine();

                }
            } while (text != "X");
            */
        }
    }
}