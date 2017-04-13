using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using yupisoft.ConfigServer.Client;

namespace ConsoleApp4
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press Enter to start");
            Console.ReadLine();

            string text = "";
            do
            {
                Console.WriteLine("Creating client...");
                //ConfigService cfg = new ConfigService("http://localhost:27764", "",1);
                ConfigService cfg = new ConfigService("http://localhost:8002", "", "3", "a1", "dG9tYXRl", true);
                //ConfigService cfg = new ConfigService("http://localhost:8002", "", "3");
                Console.WriteLine("Requesting Path: Configuration");
                try
                {
                    dynamic obj = cfg.Get<dynamic>("configuration");
                    string content = JsonConvert.SerializeObject(obj, Formatting.Indented);
                    Console.WriteLine(content);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                
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
        }
    }
}
