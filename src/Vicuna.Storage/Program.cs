using System;
using System.Diagnostics;
using System.IO;
using Vicuna.Storage.Trees;
using System.Collections.Generic;
using Vicuna.Storage.Pages.MMap;
using Vicuna.Storage.Pages;

namespace Vicuna.Storage
{
    class Program
    {
        static void Main(string[] args)
        {
            var slicePage = new StorageSlicePage(new byte[16 * 1024]);

            for (var i = 0; i < 1024; i++)
            {
                slicePage.SetEntry(i, new StorageSpaceEntry(1024 + i, 64));
            }

            var pp = new StorageFilePager(0, new StorageFile(new FileStream(@"1.txt", FileMode.OpenOrCreate)));

            var yx = new StorageSliceManager(null);

            var yx2 = new StorageSegmentManager(yx);
            //var slice = yx.AllocateSlice();
            var segment = yx2.Allocate();

            AllocationBuffer buffer = null;

            try
            {
                //var x = slice.Allocate(160, out buffer);
                //var y = slice.Allocate(160, out buffer);
            }
            catch (Exception e)
            {

            }
            var buffers = new List<AllocationBuffer>();
            var st = new Stopwatch();
            st.Start();
            for (var i = 0; i < 1000000; i++)
            {
                if (segment.Allocate(128, out buffer))
                {
                    buffers.Add(buffer);
                }
            }

            st.Stop();
            Console.WriteLine(st.ElapsedMilliseconds);
        }
    }
}