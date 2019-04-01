using System;
using System.Collections.Generic;
using System.Linq;

namespace Vicuna.Storage.Paging
{
    public class VicunaPageAllcator : IPageAllocator
    {
        public IPager Pager { get; }

        public VicunaPageAllcator(IPager pager)
        {
            Pager = pager ?? throw new ArgumentNullException(nameof(pager));
        }

        public PageIdentity Allocate()
        {
            return Allocate(1).FirstOrDefault();
        }

        public PageIdentity[] Allocate(uint count)
        {
            var start = Pager.Count;
            var pages = new PageIdentity[count];

            for (var i = 0; i < count; i++)
            {
                pages[i] = new PageIdentity(Pager.Id, start + i);
            }

            Pager.AddPage(count);

            return pages;
        }

        public void Free(long pageNumber)
        {
            Console.WriteLine($"free page:{pageNumber}");
        }

        public void Free(IEnumerable<long> pageNumbers)
        {
            foreach (var item in pageNumbers)
            {
                Free(item);
            }
        }

        public void Free(PageIdentity page)
        {
            Free(page.PageNumber);
        }

        public void Free(IEnumerable<PageIdentity> pages)
        {
            foreach (var item in pages)
            {
                Free(item);
            }
        }

        protected virtual uint GetPreAllocatedPageCount(uint count)
        {
            var size = Pager.Count * Constants.PageSize;
            if (size < 1024 * 1024)
            {
                return Math.Max(count, 8);      //128KB
            }

            if (size < 8 * 1024 * 1024)
            {
                return Math.Max(count, 16);     //256KB
            }

            if (size < 32 * 1024 * 1024)
            {
                return Math.Max(count, 32);     //512KB
            }

            if (size < 128 * 1024 * 1024)
            {
                return Math.Max(count, 128);    //2MB
            }

            if (size < 256 * 1024 * 1024)
            {
                return Math.Max(count, 256);    //4MB
            }

            if (size < 512 * 1024 * 1024)
            {
                return Math.Max(count, 1024);   //16MB
            }

            return Math.Max(count, 32);         //32MB
        }
    }
}
