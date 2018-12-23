using System;
using System.Diagnostics;
using System.IO;
using Vicuna.Storage.Trees;
using System.Collections.Generic;
using Vicuna.Storage.Pages.MMap;
using Vicuna.Storage.Pages;

namespace Vicuna.Storage
{
    unsafe class Program
    {
        static void Main(string[] args)
        {
            //var slicePage = new StorageSlicePage(new byte[16 * 1024]);

            //for (var i = 0; i < 1024; i++)
            //{
            //    slicePage.SetEntry(64 + (i * 10), new StorageSpaceEntry(1024 + i, 64));
            //}

            //var pp = new StorageFilePager(0, new StorageFile(new FileStream(@"2.txt", FileMode.OpenOrCreate)));

            ////var yx = new StorageSliceManager(null);

            ////var yx2 = new StorageSegmentManager(yx);
            //////var slice = yx.AllocateSlice();
            ////var segment = yx2.Allocate();

            ////AllocationBuffer buffer = null;

            ////try
            ////{
            ////    //var x = slice.Allocate(160, out buffer);
            ////    //var y = slice.Allocate(160, out buffer);
            ////}
            ////catch (Exception e)
            ////{

            ////}
            //var buffers = new List<AllocationBuffer>();
            var st = new Stopwatch();
            //var tx = new Transactions.StorageLevelTransaction(new Transactions.StorageLevelTransactionPageBuffer(
            //    pp
            //    ));
            //var sliceManager = new StorageSliceManager(tx);
            //var slice = sliceManager.Allocate();
            var x = new StorageSliceUsageNode(new byte[16 * 1024]);

            st.Start();
            for (var i = 0; i < 1000; i++)
            {
                x.Insert(new StorageSliceSpaceUsage() { PageOffset = i, UsedLength = (short)i });
            }

            st.Stop();

            Console.WriteLine(st.ElapsedTicks * 1.0 / Stopwatch.Frequency);
            Console.WriteLine(st.ElapsedMilliseconds);

            //var entries = x.GetEntries();

            st.Restart();


            for (var i = 0; i < 1000; i++)
            {
                x.Delete(0);
            }

            //for (var i = 0; i < 100; i++)
            //{
            //    slice.Allocate(1024, out var xxx);
            //    //if (segment.Allocate(128, out buffer))
            //    //{
            //    //    buffers.Add(buffer);
            //    //}
            //}
            st.Stop();
            //entries = x.GetEntries();
            Console.WriteLine(st.ElapsedTicks * 1.0 / Stopwatch.Frequency);
            Console.WriteLine(st.ElapsedMilliseconds);
        }
    }
}