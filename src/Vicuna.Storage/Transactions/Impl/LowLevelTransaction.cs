using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Vicuna.Storage.Data;
using Vicuna.Storage.Data.Trees;
using Vicuna.Storage.Extensions;
using Vicuna.Storage.Paging;
using Vicuna.Storage.Stores.Paging;

namespace Vicuna.Storage.Transactions.Impl
{
    public class LowLevelTransaction : ILowLevelTransaction
    {
        private object _syncRoot;

        public long Id { get; internal set; }

        public TransactionState State { get; internal set; }

        protected internal IPageBufferPool Pool { get; }

        protected internal IStorePageManager PageManager { get; }

        protected internal HashSet<PageNumberInfo> AllocatedPages { get; }

        protected internal ConcurrentDictionary<PageNumberInfo, Page> ModifiedPages { get; }

        protected internal ConcurrentDictionary<int, List<PageNumberInfo>> UnUsedPages { get; }

        Transactions.TransactionState ILowLevelTransaction.State => throw new NotImplementedException();

        public LowLevelTransaction(IPageBufferPool pageBufferPool, IStorePageManager pageManager)
        {
            _syncRoot = new object();
            Pool = pageBufferPool;
            PageManager = pageManager;
            AllocatedPages = new HashSet<PageNumberInfo>();
            UnUsedPages = new ConcurrentDictionary<int, List<PageNumberInfo>>();
            ModifiedPages = new ConcurrentDictionary<PageNumberInfo, Page>();
        }

        public Page AllocatePage(int token)
        {
            return AllocatePage(token, 1).FirstOrDefault();
        }

        public Page[] AllocatePage(int token, uint count)
        {
            var pageIdentities = AllocateUnUsedPages(token, (int)count);
            if (pageIdentities.Count < (int)count)
            {
                var newPages = AllocateNewPages(token, (int)count - pageIdentities.Count);
                if (newPages != null)
                {
                    pageIdentities.AddRange(newPages);
                }
            }

            var pages = new Page[count];

            for (var i = 0; i < count; i++)
            {
                pages[i] = ModifyPage(pageIdentities[i]);
            }

            return pages;
        }

        public IEnumerable<PageNumberInfo> AllocateNewPages(int token, int count)
        {
            var newPages = PageManager.Allocate(token, (uint)count);
            if (newPages != null)
            {
                AllocatedPages.AddRange(newPages);
            }

            return newPages;
        }

        public List<PageNumberInfo> AllocateUnUsedPages(int token, int count)
        {
            if (!UnUsedPages.TryGetValue(token, out var list))
            {
                return new List<PageNumberInfo>();
            }

            if (list.Count > count)
            {
                UnUsedPages.TryRemove(token, out var _);
                return list;
            }

            var pages = list.Take(count).ToList();

            list.RemoveRange(0, count);

            return pages;
        }

        public Page GetPage(PageNumberInfo number)
        {
            if (ModifiedPages.TryGetValue(number, out var dirtyPage))
            {
                return dirtyPage;
            }

            if (AllocatedPages.Contains(number))
            {
                return CreatePage(number);
            }

            var poolPage = Pool.GetEntry(number);
            if (poolPage != null)
            {
                return poolPage;
            }

            var page = PageManager.GetPage(number);
            if (page == null)
            {
                throw null;
            }

            Pool.AddEntry(page, PageEntryState.None);

            return page;
        }

        public Page ModifyPage(PageNumberInfo number)
        {
            if (ModifiedPages.TryGetValue(number, out var page))
            {
                return page;
            }

            var oldPage = GetPage(number);
            var newPage = ModifyPage(oldPage);

            if (!ModifiedPages.TryAdd(number, newPage))
            {
                throw new InvalidOperationException($"modify page {number.PageNumber} failed!");
            }

            return newPage;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected internal Page ModifyPage(Page oldPage)
        {
            var newPage = new Page();

            Array.Copy(oldPage.Data, 0, newPage.Data, 0, Constants.PageSize);

            return newPage;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected internal Page CreatePage(PageNumberInfo number)
        {
            var page = new Page();
            ref var header = ref page.PageHeader;

            header.PagerId = number.StoreId;
            header.PageNumber = number.PageNumber;

            return page;
        }

        public void Commit()
        {
            lock (_syncRoot)
            {
                CheckTransactionState();
                State = TransactionState.Commited;

                foreach (var item in ModifiedPages)
                {
                    PageManager.SetPage(item.Key, item.Value);
                }
            }
        }

        public void Rollback()
        {
            lock (_syncRoot)
            {
                CheckTransactionState();
                State = TransactionState.Aborted;
            }
        }

        private void CheckTransactionState()
        {
            switch (State)
            {
                case TransactionState.Aborted:
                    throw new InvalidOperationException($"transaction{Id} has been commited!");
                case TransactionState.Commited:
                    throw new InvalidOperationException($"transaction{Id} has been commited!");
            }
        }

        public void Dispose()
        {

        }

        public Tree OpenTree(EncodingByteString name)
        {
            throw new NotImplementedException();
        }

        public Tree CreateTree(EncodingByteString name)
        {
            throw new NotImplementedException();
        }

        public void WritePageLog(Page page)
        {
            throw new NotImplementedException();
        }
    }

    public enum TransactionState
    {
        Waitting,

        Running,

        Aborted,

        Commited
    }
}
