using System;
using System.Diagnostics;
using System.IO;
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
            var y1 = 20000;
            for (var i = 0; i < y1; i++)
            {
                if (i.ToString() == "142795")
                {

                }
                var keyString = i.ToString();
                var size = Encoding.UTF8.GetBytes(keyString);
                Span<byte> span = new byte[size.Length + 1];

                span[0] = (byte)size.Length;

                size.AsSpan().CopyTo(span.Slice(1));

                tree.Insert(new Data.Trees.TreeNodeKey(span), new Data.Trees.TreeNodeValue(BitConverter.GetBytes(i).AsSpan()), Data.Trees.TreeNodeHeaderFlags.Data);

            };

            st.Start();
            var str = new List<string>();
            for (var i = 0; i < y1; i++)
            {
                try
                {
                    var keyString = i.ToString();
                    var size = Encoding.UTF8.GetBytes(keyString);
                    Span<byte> span = new byte[size.Length + 1];

                    span[0] = (byte)size.Length;

                    size.AsSpan().CopyTo(span.Slice(1));
                    var key = new Data.Trees.TreeNodeKey(span);
                    var value = tree.Get(key);
                    BitConverter.ToInt32(value.Values);
                    Console.WriteLine(BitConverter.ToInt32(value.Values));
                }
                catch
                {
                    str.Add(i.ToString());
                }
                if (i.ToString() == "142794")
                {
                    break;
                }
                //if (value.Size > 0)
                //    Console.WriteLine(BitConverter.ToInt32(value.Values));
            }
            Console.WriteLine(str.Count + "_");
            tx.Dispose();
            st.Stop();
            Console.WriteLine(st.ElapsedMilliseconds * 1.0 / 1000000);
            Console.WriteLine(st.ElapsedMilliseconds * 1.0);

        }
    }
}