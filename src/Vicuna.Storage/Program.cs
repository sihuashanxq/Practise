using System;
using System.Diagnostics;
using System.IO;
using Vicuna.Storage.Trees;
using System.Collections.Generic;
using Vicuna.Storage.Pages;
using Vicuna.Storage.Transactions;
using System.Text;
using System.Runtime.CompilerServices;
using System.Reflection.Emit;

namespace Vicuna.Storage
{
    public struct Method
    {
        public long Id;

        public long Age;
    }

    unsafe class Program
    {
        public static ref byte Get(ref byte b)
        {
            return ref b;
        }

        static void Main(string[] args)
        {
            var x = new byte[1024];
            var n = new byte[32];
            var st = new Stopwatch();

            Array.Copy(x, 0, n, 0, 32);
            Unsafe.CopyBlockUnaligned(ref x[0], ref n[0], 16);


            st.Start();
            for(var i = 0; i < 10000000; i++)
            {
                Array.Copy(x, 0, n, 0, 16);
            }
            st.Stop();
            Console.WriteLine(st.ElapsedMilliseconds);

            st.Reset();
            st.Start();
            for (var i = 0; i < 10000000; i++)
            {
                Unsafe.CopyBlockUnaligned(ref n[0], ref x[0], 16);
            }
            st.Stop();
            Console.WriteLine(st.ElapsedMilliseconds);


            //var pp = new StorageFilePageManager(1024 * 100, new StorageFile(new FileStream(@"4.txt", FileMode.OpenOrCreate)));
            //var st = new Stopwatch();
            //var tx = new StorageLevelTransaction(new Transactions.StorageLevelTransactionBufferPool(pp));
            //var bytes = Encoding.UTF8.GetBytes("what are your name!");

            //st.Start();
            //for (var i = 0; i < 1024 * 100; i++)
            //{
            //    var p = tx.GetPage(i);
            //}

            //tx.Dispose();
            //st.Stop();
            //Console.WriteLine(st.ElapsedTicks * 1.0 / Stopwatch.Frequency);
        }
    }
}