using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using yupisoft.ConfigServer.Client;

namespace ConsoleApp1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            

            string text = "";
            do
            {
                ConfigService cfg = new ConfigService("http://localhost:27764", "",1);
                dynamic obj = cfg.Get<dynamic>("configuration");
                string content = JsonConvert.SerializeObject(obj, Formatting.Indented);

                Console.WriteLine(content);
                Console.WriteLine("=============================");
                Console.WriteLine("Enter 'X' to Exit, Y to modify an element");
                text = Console.ReadLine();


                if (text == "Y")
                {
                    obj.culo = "sucio";
                    cfg.Set<dynamic>("configuration", obj);
                    Console.WriteLine("... MODIFIED ...");
                    Console.WriteLine("=============================");
                    Console.WriteLine("Enter 'X' to Exit, Y to modify an element");
                    text = Console.ReadLine();

                }
            } while (text != "X");
        }

    }
}
