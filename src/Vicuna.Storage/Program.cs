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
            var pp = new StorageFilePageManager(1024 * 100, new StorageFile(new FileStream(@"4.txt", FileMode.OpenOrCreate)));
            var st = new Stopwatch();
            var tx = new StorageLevelTransaction(new Transactions.StorageLevelTransactionBufferPool(pp));
            var bytes = Encoding.UTF8.GetBytes("what are your name!");
            var tree = new Data.Trees.Tree()
            {
                _tx = tx
            };

            var mixLen = 10;

            for (var i = 0; i < 200; i++)
            {
                if (i == 115)
                {

                }
                var keyString = i.ToString();
                var size = Encoding.UTF8.GetBytes(keyString);
                Span<byte> span = new byte[size.Length + 1];

                span[0] = (byte)size.Length;

                size.AsSpan().CopyTo(span.Slice(1));

                tree.Insert(new Data.Trees.TreeNodeKey(span), new Data.Trees.TreeNodeValue(BitConverter.GetBytes(i).AsSpan()), Data.Trees.TreeNodeHeaderFlags.Data);
            }

            st.Start();
            var str = new List<string>();
            for (var i = 0; i < 200; i++)
            {
                var value = tree._root.GetNodeKey(i);
                str.Add(System.Text.Encoding.UTF8.GetString(value.Keys.Slice(1)));
                Console.WriteLine();
            }

            for (var n = 0; n < 1000; n++)
            {
                var i = n % 200;
                var keyString = i.ToString();
                var size = Encoding.UTF8.GetBytes(keyString);
                Span<byte> span = new byte[size.Length + 1];

                span[0] = (byte)size.Length;

                size.AsSpan().CopyTo(span.Slice(1));

                var value = tree.Get(new Data.Trees.TreeNodeKey(span));
                Console.WriteLine(BitConverter.ToInt32(value.Values));
                //if (value.Size > 0)
                //    Console.WriteLine(BitConverter.ToInt32(value.Values));
            }

            tx.Dispose();
            st.Stop();
            Console.WriteLine(st.ElapsedMilliseconds * 1.0 / 1000000);
        }
    }
}