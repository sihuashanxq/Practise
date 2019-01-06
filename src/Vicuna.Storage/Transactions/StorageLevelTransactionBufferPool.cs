using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Vicuna.Storage.Pages;

namespace Vicuna.Storage.Transactions
{
    public unsafe class StorageLevelTransactionBufferPool : IDisposable
    {
        public HashSet<long> UnUnsedPages { get; }

        public HashSet<long> AllocatedPages { get; }

        public StorageFilePageManager PageManager { get; }

        public ConcurrentDictionary<long, Page> ModifiedPages { get; }

        public StorageLevelTransactionBufferPool(StorageFilePageManager pageManager)
        {
            PageManager = pageManager;
            UnUnsedPages = new HashSet<long>();
            AllocatedPages = new HashSet<long>();
            ModifiedPages = new ConcurrentDictionary<long, Page>();
        }

        public bool TryGetPage(long pageNumber, out Page page)
        {
            if (ModifiedPages.TryGetValue(pageNumber, out page))
            {
                return true;
            }

            if (AllocatedPages.Contains(pageNumber))
            {
                page = new Page(pageNumber);
                ModifiedPages.TryAdd(pageNumber, page);
                return true;
            }

            var buffer = PageManager.GetPage(pageNumber);
            if (buffer == null)
            {
                page = null;
                return false;
            }

            page = new Page(buffer);
            return true;
        }

        public bool TryGetPageToModify(long pageNumber, out Page modifedPage)
        {
            if (ModifiedPages.TryGetValue(pageNumber, out modifedPage))
            {
                return true;
            }

            if (AllocatedPages.Contains(pageNumber))
            {
                modifedPage = new Page(pageNumber);
                ModifiedPages.TryAdd(pageNumber, modifedPage);
                return true;
            }

            var buffer = PageManager.GetPage(pageNumber);
            if (buffer != null)
            {
                modifedPage = new Page(buffer).Clone();
                ModifiedPages.TryAdd(pageNumber, modifedPage);
                return true;
            }

            modifedPage = null;
            return false;
        }

        public bool TryAllocateSlicePage(out long pageNumber)
        {
            var sliceHeadPageNumber = PageManager.AppendPage(Constants.SlicePageCount);
            if (sliceHeadPageNumber < 0)
            {
                pageNumber = -1;
                return false;
            }

            for (var i = 0; i < Constants.SlicePageCount; i++)
            {
                AllocatedPages.Add(sliceHeadPageNumber + i);
            }

            pageNumber = sliceHeadPageNumber;
            return true;
        }

        public void Dispose()
        {
            foreach (var item in ModifiedPages)
            {
                PageManager.SetPage(item.Key, item.Value.Buffer);
            }

            PageManager.Dispose();
        }
    }
}
