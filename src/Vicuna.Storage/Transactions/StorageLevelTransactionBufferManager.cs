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

        public bool TryGetPage(long pageOffset, out Page page)
        {
            if (ModifiedPages.TryGetValue(pageOffset, out page))
            {
                return true;
            }

            if (AllocatedPages.Contains(pageOffset))
            {
                page = new Page(pageOffset);
                return true;
            }

            page = PaginationManager.GetPage(pageOffset);
            return page != null;
        }

        public bool TryGetPageToModify(long pageOffset, out Page modifedPage)
        {
            if (ModifiedPages.TryGetValue(pageOffset, out modifedPage))
            {
                return true;
            }

            if (AllocatedPages.Contains(pageOffset))
            {
                modifedPage = new Page(pageOffset);
                ModifiedPages.TryAdd(pageOffset, modifedPage);
                return true;
            }

            var page = PaginationManager.GetPage(pageOffset);
            if (page == null)
            {
                return false;
            }

            modifedPage = page.Clone();
            ModifiedPages.TryAdd(pageOffset, modifedPage);
            return true;
        }

        public long AllocateSlicePage()
        {
            var sliceHeadPageOffset = PaginationManager.Allocate(Constants.SlicePageCount);
            if (sliceHeadPageOffset < 0)
            {
                throw new IndexOutOfRangeException(nameof(sliceHeadPageOffset));
            }

            for (var i = 0; i < Constants.SlicePageCount; i++)
            {
                AllocatedPages.Add(sliceHeadPageOffset + i);
            }

            return sliceHeadPageOffset;
        }
    }
}
