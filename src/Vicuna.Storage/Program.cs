using System;
using System.Diagnostics;
using System.IO;
using Vicuna.Storage.Trees;
using System.Collections.Generic;
using Vicuna.Storage.Pages;
using Vicuna.Storage.Transactions;
using System.Text;

namespace Vicuna.Storage
{
    unsafe class Program
    {
        static void Main(string[] args)
        {
            var pp = new StorageFilePageManager(1024 * 100, new StorageFile(new FileStream(@"4.txt", FileMode.OpenOrCreate)));
            var st = new Stopwatch();
            var tx = new StorageLevelTransaction(new Transactions.StorageLevelTransactionBufferPool(pp));
            var bytes = Encoding.UTF8.GetBytes("what are your name!");

            st.Start();
            for (var i = 0; i < 1024 * 100; i++)
            {
                var p = tx.GetPage(i);
            }

            tx.Dispose();
            st.Stop();
            Console.WriteLine(st.ElapsedTicks * 1.0 / Stopwatch.Frequency);
        }
    }
}