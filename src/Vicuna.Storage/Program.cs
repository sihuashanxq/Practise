using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using Vicuna.Storage.Pages;
using Vicuna.Storage.Transactions;
using System.Text;
using System.Runtime.CompilerServices;
using System.Reflection.Emit;
using Vicuna.Storage.Data.Trees;

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
            var tree = new Data.Trees.Tree()
            {
                _tx = tx
            };

            var mixLen = 10;
            var count = 200000;

            for (var i = 0; i < count; i++)
            {
                Span<byte> bytes = Encoding.UTF8.GetBytes(i.ToString());
                Span<byte> key = new byte[bytes.Length + 1];

                //value
                Span<byte> value = Encoding.UTF8.GetBytes(("say :" + i).ToString ());

                //set key size
                key[0] = (byte)bytes.Length;

                bytes.CopyTo(key.Slice(1));

                tree.Insert(new TreeNodeKey(key), new TreeNodeValue(value), Data.Trees.TreeNodeHeaderFlags.Data);
         
            };

            st.Start();
            var str = new List<string>();
            for (var i = 0; i < count; i++)
            {
                try
                {
                    Span<byte> bytes = Encoding.UTF8.GetBytes(i.ToString());
                    Span<byte> key = new byte[bytes.Length + 1];

                    //set key size
                    key[0] = (byte)bytes.Length;

                    bytes.CopyTo(key.Slice(1));

                    var value = tree.Get(new TreeNodeKey(key));

                    //Console.WriteLine(Encoding.UTF8.GetString(value.Values));

                    str.Add(BitConverter.ToInt32(value.Values).ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("err" + i);
                }
            }
            Console.WriteLine(str.Count + "_");
            tx.Dispose();
            st.Stop();
            Console.WriteLine(st.ElapsedMilliseconds * 1.0 / 1000000);
            Console.WriteLine(st.ElapsedMilliseconds * 1.0);

        }
    }
}