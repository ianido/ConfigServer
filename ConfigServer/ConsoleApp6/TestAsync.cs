using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using yupisoft.ConfigServer.Core;

namespace ConsoleApp6
{
    public class TestAsync
    {

        public static void RunMethod()
        {
            bool forcefinish = false;
            Task tsk = Task.Factory.StartNew(() =>
            {
                Console.Write("Running.");
                for(int i=0; i< 100; i++)
                {
                    //if (forcefinish) break;
                    Thread.Sleep(100);
                    Console.Write(".");
                }
            }).WithTimeout(TimeSpan.FromSeconds(5)).ContinueWith((t) => {
                forcefinish = true;
                Console.WriteLine("");
                Console.WriteLine("Finished Status:" + t.Status.ToString());
                Console.WriteLine("Finished Exception:" + t.Exception?.ToString());
            });
            Console.WriteLine();
            Console.WriteLine("After the task.");
        }

        public static void RunMethod1()
        {
            var c = new CancellationTokenSource(3000).Token;
            c.Register(() => {
                Console.WriteLine("Time Out.");
            });
            Task tsk = Task.Factory.StartNew((ct) =>
            {
                Console.Write("Running.");
                for (int i = 0; i < 100; i++)
                {
                    if (((CancellationToken)ct).IsCancellationRequested) return;
                    Thread.Sleep(100);
                    Console.Write(".");
                }
            }, c).ContinueWith((t) => {
                Console.WriteLine("");
                Console.WriteLine("Finished: " + t.AsyncState + " Status:" + t.Status.ToString());

            });
            Console.WriteLine();
            Console.WriteLine("After the task.");
        }


    }
}
