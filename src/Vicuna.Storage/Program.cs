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
            var slice = new StorageSlice(slicePage, pp);

            AllocationBuffer buffer = null;

            var x = slice.Allocate(160, out buffer);
            var y = slice.Allocate(160, out buffer);
            var xs = new List<AllocationBuffer>();

            for (var i = 0; i < 17; i++)
            {
                slice.Allocate(1024, out buffer);
                xs.Add(buffer);
            }
        }
    }
}