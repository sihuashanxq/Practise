using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Vicuna.Storage.Pages;

namespace Vicuna.Storage.Transactions
{
    public unsafe class StorageLevelTransactionBufferManager
    {
        internal HashSet<long> UnUnsedPages { get; }

        internal HashSet<long> AllocatedPages { get; }

        internal StoragePaginationManager PaginationManager { get; }

        internal ConcurrentDictionary<long, Page> ModifiedPages { get; }

        public StorageLevelTransactionBufferManager(StoragePaginationManager paginationManager)
        {
            PaginationManager = paginationManager;
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

            page = PaginationManager.GetPage(pageNumber);
            return page != null;
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

            var page = PaginationManager.GetPage(pageNumber);
            if (page == null)
            {
                return false;
            }

            modifedPage = page.Clone();
            ModifiedPages.TryAdd(pageNumber, modifedPage);
            return true;
        }

        public bool TryAllocateSlicePage(out long pageNumber)
        {
            var sliceHeadPageNumber = PaginationManager.Allocate(Constants.SlicePageCount);
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
    }
}
