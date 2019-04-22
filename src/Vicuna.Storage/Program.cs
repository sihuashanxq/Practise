using System;
using System.Collections.Generic;
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

            manager._pagerMaps[0] = new Pager(0, new FileStore("1.txt"));

            var tx = new LowLevelTransaction(pool, manager);
            var tree = new Tree();

            tree.Init(tx);

            tx.Commit();

            for (var i = 0; i < 3000000; i++)
            {
                Span<byte> bytes = Encoding.UTF8.GetBytes(i.ToString());
                Span<byte> key = new byte[bytes.Length + 2];

                //value
                var value = Encoding.UTF8.GetBytes(("say :" + i).ToString());
                var values = new byte[value.Length + 2];
                values[0] = (byte)DataValueType.String;
                values[1] = (byte)value.Length;
                //set key size
                key[0] = (byte)DataValueType.String;
                key[1] = (byte)bytes.Length;

                bytes.CopyTo(key.Slice(2));

                tree.Insert(new TreeNodeDataEntry()
                {
                    Key = new TreeNodeDataSlice(key, TreeNodeDataSliceType.Key),
                    Value = new TreeNodeDataSlice(value, TreeNodeDataSliceType.Value)
                }, tx);
            };

            var err = new List<string>();

            for (var i = 0; i < 3000000; i++)
            {
                try
                {
                    Span<byte> bytes = Encoding.UTF8.GetBytes(i.ToString());
                    Span<byte> key = new byte[bytes.Length + 2];

                    //value
                    //set key size
                    key[0] = (byte)DataValueType.String;
                    key[1] = (byte)bytes.Length;

                    bytes.CopyTo(key.Slice(2));

                    var value = tree.GetValue(new TreeNodeDataSlice(key, TreeNodeDataSliceType.Key), tx);
                    if (value.Size == 0)
                    {
                        err.Add(i.ToString());
                    }
                    //Console.WriteLine(value.ToString());
                }
                catch (Exception e)
                {
                    err.Add(i.ToString());
                }
            }

            Console.WriteLine(err.Count);

            var n = 0;

            //var pp = new StorageFilePageManager(1024 * 100, new StorageFile(new FileStream(@"4.txt", FileMode.OpenOrCreate)));
            //var st = new Stopwatch();
            //var tx = new StorageLevelTransaction(new Transactions.StorageLevelTransactionBufferPool(pp));
            //var tree = new Data.Trees.Tree()
            //{
            //    _tx = tx
            //};

            //var mixLen = 10;
            //var count = 200000;

            //for (var i = 0; i < count; i++)
            //{
            //    Span<byte> bytes = Encoding.UTF8.GetBytes(i.ToString());
            //    Span<byte> key = new byte[bytes.Length + 1];

            //    //value
            //    Span<byte> value = Encoding.UTF8.GetBytes(("say :" + i).ToString ());

            //    //set key size
            //    key[0] = (byte)bytes.Length;

            //    bytes.CopyTo(key.Slice(1));

            //    tree.Insert(new TreeNodeKey(key), new TreeNodeValue(value), Data.Trees.TreeNodeHeaderFlags.Data);

            //};

            //st.Start();
            //var str = new List<string>();
            //for (var i = 0; i < count; i++)
            //{
            //    try
            //    {
            //        Span<byte> bytes = Encoding.UTF8.GetBytes(i.ToString());
            //        Span<byte> key = new byte[bytes.Length + 1];

            //        //set key size
            //        key[0] = (byte)bytes.Length;

            //        bytes.CopyTo(key.Slice(1));

            //        var value = tree.Get(new TreeNodeKey(key));

            //        //Console.WriteLine(Encoding.UTF8.GetString(value.Values));

            //        str.Add(BitConverter.ToInt32(value.Values).ToString());
            //    }
            //    catch (Exception e)
            //    {
            //        Console.WriteLine("err" + i);
            //    }
            //}
            //Console.WriteLine(str.Count + "_");
            //tx.Dispose();
            //st.Stop();
            //Console.WriteLine(st.ElapsedMilliseconds * 1.0 / 1000000);
            //Console.WriteLine(st.ElapsedMilliseconds * 1.0);
        }
    }
}