using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class Program
    {
        static Timer timer { get; set; }
        public static void Main(string[] args)
        {
            timer = new Timer(new TimerCallback(tmCallback), "abc", 2000, 1000);
            Console.WriteLine("hey");
            Console.ReadLine();
        }

        public static void tmCallback(object state)
        {
            timer.Change(Timeout.Infinite, 1000);
            Console.Write("gatico: ");
            Thread.Sleep(3000);
            Console.WriteLine(state);
            timer.Change(0, 1000);
        }
    }
}
