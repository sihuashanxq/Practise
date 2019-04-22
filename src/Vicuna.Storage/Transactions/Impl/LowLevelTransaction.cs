using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Vicuna.Storage.Extensions;
using Vicuna.Storage.Paging;

namespace Vicuna.Storage.Transactions.Impl
{
    public class LowLevelTransaction : ILowLevelTransaction
    {
        private object _syncRoot;

        public long Id { get; internal set; }

        public TransactionState State { get; internal set; }

        protected internal IPageBufferPool Pool { get; }

        protected internal IPageManager PageManager { get; }

        protected internal HashSet<PageIdentity> AllocatedPages { get; }

        protected internal ConcurrentDictionary<int, List<PageIdentity>> UnUsedPages { get; }

        protected internal ConcurrentDictionary<PageIdentity, Page> ModifiedPages { get; }

        Transactions.TransactionState ILowLevelTransaction.State => throw new NotImplementedException();

        public LowLevelTransaction(IPageBufferPool pageBufferPool, IPageManager pageManager)
        {
            _syncRoot = new object();
            Pool = pageBufferPool;
            PageManager = pageManager;
            AllocatedPages = new HashSet<PageIdentity>();
            UnUsedPages = new ConcurrentDictionary<int, List<PageIdentity>>();
            ModifiedPages = new ConcurrentDictionary<PageIdentity, Page>();
        }

        public PageIdentity AllocatePage(int token)
        {
            return AllocatePage(token, 1).FirstOrDefault();
        }

        public List<PageIdentity> AllocatePage(int token, uint count)
        {
            var unusedPages = AllocateWithUnUsedPages(token, (int)count);
            if (unusedPages.Count == count)
            {
                return unusedPages;
            }

            var newPages = AllocateWithNewPages(token, (int)count - unusedPages.Count);
            if (newPages != null)
            {
                unusedPages.AddRange(newPages);
            }

            return unusedPages;
        }

        public IEnumerable<PageIdentity> AllocateWithNewPages(int token, int count)
        {
            var newPages = PageManager.Allocate(token, (uint)count);
            if (newPages != null)
            {
                AllocatedPages.AddRange(newPages);
            }

            return newPages;
        }

        public List<PageIdentity> AllocateWithUnUsedPages(int token, int count)
        {
            if (!UnUsedPages.TryGetValue(token, out var list))
            {
                return new List<PageIdentity>();
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

        public Page GetPage(PageIdentity identity)
        {
            if (ModifiedPages.TryGetValue(identity, out var dirtyPage))
            {
                return dirtyPage;
            }

            if (AllocatedPages.Contains(identity))
            {
                return CreatePage(identity);
            }

            var poolPage = Pool.GetEntry(identity);
            if (poolPage != null)
            {
                return poolPage;
            }

            var page = PageManager.GetPage(identity);
            if (page == null)
            {
                throw null;
            }

            Pool.AddEntry(page, PageEntryState.None);

            return page;
        }

        public Page GetPage(int pagerId, long pageNumber)
        {
            return GetPage(new PageIdentity(pagerId, pageNumber));
        }

        public Page ModifyPage(PageIdentity identity)
        {
            if (ModifiedPages.TryGetValue(identity, out var page))
            {
                return page;
            }

            var oldPage = GetPage(identity);
            var newPage = ModifyPage(oldPage);

            if (!ModifiedPages.TryAdd(identity, newPage))
            {
                throw new InvalidOperationException($"modify page {identity.PageNumber} failed!");
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
        protected internal Page CreatePage(PageIdentity identity)
        {
            var page = new Page();
            ref var header = ref page.PageHeader;

            header.PagerId = identity.PagerId;
            header.PageNumber = identity.PageNumber;

            return page;
        }

        public void Commit()
        {
            lock (_syncRoot)
            {
                CheckTransactionState();
                State = TransactionState.Commited;
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
    }

    public enum TransactionState
    {
        Waitting,

        Running,

        Aborted,

        Commited
    }
}
