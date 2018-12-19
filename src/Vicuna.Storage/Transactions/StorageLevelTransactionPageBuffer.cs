using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Vicuna.Storage.Pages;

namespace Vicuna.Storage.Transactions
{
    public class StorageLevelTransactionPageBuffer
    {
        private HashSet<long> UnUnsedPages { get; }

        private HashSet<long> AllocatedPages { get; }

        private StoragePageManager PageManager { get; }

        private ConcurrentDictionary<long, byte[]> ModifiedPages { get; }

        public StorageLevelTransactionPageBuffer(StoragePageManager pageManager)
        {
            PageManager = pageManager;
            UnUnsedPages = new HashSet<long>();
            AllocatedPages = new HashSet<long>();
            ModifiedPages = new ConcurrentDictionary<long, byte[]>();
        }

        public bool TryGetPage(long pageOffset, out byte[] pageContent)
        {
            if (ModifiedPages.TryGetValue(pageOffset, out pageContent))
            {
                return true;
            }

            if (AllocatedPages.Contains(pageOffset))
            {
                pageContent = new byte[Constants.PageSize];
                return true;
            }

            pageContent = PageManager.GetPageContent(pageOffset);

            return pageContent != null;
        }

        public bool TryGetPageToModify(long pageOffset, out byte[] pageContent)
        {
            if (ModifiedPages.TryGetValue(pageOffset, out pageContent))
            {
                return true;
            }

            if (AllocatedPages.Contains(pageOffset))
            {
                pageContent = new byte[Constants.PageSize];
                ModifiedPages.TryAdd(pageOffset, pageContent);
                return true;
            }

            var page = PageManager.GetPageContent(pageOffset);
            if (page == null)
            {
                return false;
            }

            pageContent = new byte[page.Length];
            Array.Copy(page, page, page.Length);
            ModifiedPages.TryAdd(pageOffset, pageContent);
            return true;
        }

        public long AllocatePage()
        {
            var allocatedPage = PageManager.Allocate();
            if (allocatedPage != -1)
            {
                AllocatedPages.Add(allocatedPage);
            }

            return allocatedPage;
        }

        public long[] AllocatePage(int pageCount)
        {
            var allocatedPages = PageManager.Allocate(pageCount);
            if (allocatedPages == null)
            {
                return allocatedPages;
            }

            foreach (var item in allocatedPages)
            {
                AllocatedPages.Add(item);
            }

            return allocatedPages;
        }
    }
}
