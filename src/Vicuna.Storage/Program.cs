using System;
using System.Diagnostics;
using Vicuna.Storage.Trees;

namespace Vicuna.Storage
{
    class Program
    {
        static void Main(string[] args)
        {
            var x = new Tree<int>()
            {
                _storage = new TreeStorage<int>()
            };

            for (var i = 1000000; i >= 0; i--)
            {
                x.Set(i, i);
            }

            var stop = new Stopwatch();
            stop.Start();

            for (var i = 1000000; i >= 0; i--)
            {
                if (i > 9800 && i % 2 != 0)
                {
                    x.Remove(i);
                }
            }

            stop.Stop();
            Console.WriteLine(stop.ElapsedMilliseconds);

            for (var i = 0; i < 10000; i++)
            {
                var a = x.Get(i);
                if (a == -1)
                {
                    Console.WriteLine(i);
                }
            }
        }
    }
}