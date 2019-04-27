using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Vicuna.Storage.Data.Trees;
using Vicuna.Storage.Paging.Impl;
using Vicuna.Storage.Stores.Impl;
using Vicuna.Storage.Transactions.Impl;

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
            var manager = new PageManager();
            var pool = new PageBufferPool();
            var stop = new Stopwatch();
            var file = new FileStore("1.txt");
            manager._pagers[0] = new Pager(file);

            var tx = new LowLevelTransaction(pool, manager);
            var tree = new Tree();

            //tree.Init(tx);

            //stop.Start();
            //for (var i = 0; i < 2000000; i++)
            //{
            //    Span<byte> bytes = Encoding.UTF8.GetBytes(i.ToString());
            //    Span<byte> key = new byte[bytes.Length + 2];

            //    var value = Encoding.UTF8.GetBytes(("say :" + i).ToString());
            //    var values = new byte[value.Length + 2];
            //    values[0] = (byte)DataValueType.String;
            //    values[1] = (byte)value.Length;

            //    set key size
            //    key[0] = (byte)DataValueType.String;
            //    key[1] = (byte)bytes.Length;

            //    bytes.CopyTo(key.Slice(2));
            //    value.CopyTo(values.AsSpan().Slice(2));
            //    tree.Insert(tx, new TreeNodeDataEntry()
            //    {
            //        Key = new TreeNodeDataSlice(key, TreeNodeDataSliceType.Key),
            //        Value = new TreeNodeDataSlice(values, TreeNodeDataSliceType.Value)
            //    });
            //};

            //stop.Stop();
            Console.WriteLine(stop.ElapsedMilliseconds);

            tx.Commit();
            file.Sync();
            var err = new List<string>();

            for (var n = 0; n < 100; n++)
            {
                stop.Reset();
                stop.Start();

                for (var i = 0; i < 1000000; i++)
                {
                    try
                    {
                        Span<byte> bytes = Encoding.UTF8.GetBytes(i.ToString());
                        Span<byte> key = new byte[bytes.Length + 2];

                        key[0] = (byte)DataValueType.String;
                        key[1] = (byte)bytes.Length;

                        bytes.CopyTo(key.Slice(2));

                        var value = tree.GetValue(tx, new TreeNodeDataSlice(key, TreeNodeDataSliceType.Key));

                    }
                    catch (Exception e)
                    {
                        err.Add(i.ToString());
                    }
                }

                stop.Stop();
                Console.WriteLine(stop.ElapsedMilliseconds);

                Console.WriteLine(err.Count);
                Console.WriteLine(tree._root.PageNumber);
            }

            tx.Commit();
            file.Sync();
        }
    }
}