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
            ConfigService cfg = new ConfigService("http://localhost:27764", "");

            string text = "";
            do
            {
                dynamic obj = cfg.Get<dynamic>("configuration");
                string content = JsonConvert.SerializeObject(obj, Formatting.Indented);

                Console.WriteLine(content);
                Console.WriteLine("=============================");
                Console.WriteLine("Enter 'X' to Exit");
                text = Console.ReadLine();
            } while (text != "X");
        }

    }
}
