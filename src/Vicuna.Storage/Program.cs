using System;
using System.Diagnostics;
using System.IO;
using Vicuna.Storage.Trees;
using System.Collections.Generic;
using Vicuna.Storage.Pages.MMap;

namespace Vicuna.Storage
{
    class Program
    {
        static void Main(string[] args)
        {
            var slicePage = new StorageSlicePage(new byte[16 * 1024]);

            for (var i = 0; i < 1024; i++)
            {
                slicePage.AddEntry(i, new StorageSpaceUsageEntry(1024 + i, 64));
            }

            var pp = new MemoryMappedFilePageManager(new FileStream(@"1.txt", FileMode.OpenOrCreate)).Pager;
            var yx = new StorageSliceHandling(pp);

            var yx2 = new StorageSegmentHandling(yx);
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
            var xs = new List<AllocationBuffer>();

            for (var i = 0; i < 16 * 1000; i++)
            {
                segment.Allocate(1024, out buffer);
                xs.Add(buffer);
            }
        }
    }
}