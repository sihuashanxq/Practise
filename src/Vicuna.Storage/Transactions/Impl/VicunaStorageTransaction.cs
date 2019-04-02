using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Vicuna.Storage.Extensions;
using Vicuna.Storage.Paging;

namespace Vicuna.Storage.Transactions.Impl
{
    public class VicunaStorageTransaction : IStorageTransaction
    {
        private object _syncRoot;

        public long Id { get; internal set; }

        public TransactionState State { get; internal set; }

        protected internal IPageBufferPool Pool { get; }

        protected internal IPageManager PageManager { get; }

        protected internal HashSet<PageIdentity> AllocationPages { get; }

        protected internal ConcurrentDictionary<int, HashSet<PageIdentity>> UnUsingPages { get; }

        protected internal ConcurrentDictionary<PageIdentity, Page> ModificationPages { get; }

        public VicunaStorageTransaction()
        {
            _syncRoot = new object();
            AllocationPages = new HashSet<PageIdentity>();
            UnUsingPages = new ConcurrentDictionary<int, HashSet<PageIdentity>>();
            ModificationPages = new ConcurrentDictionary<PageIdentity, Page>();
        }

        public PageIdentity AllocatePage(int token)
        {
            return AllocatePage(token, 1).FirstOrDefault();
        }

        public PageIdentity[] AllocatePage(int token, uint count)
        {
            var unUses = new PageIdentity[0];
            if (UnUsingPages.TryGetValue(token, out var list))
            {
                var unUseCount = Math.Min(count, list.Count);
                if (unUseCount == list.Count)
                {
                    unUses = list.ToArray();
                    UnUsingPages.TryRemove(token, out var _);
                }
                else
                {
                    unUses = list.Take((int)unUseCount).ToArray();
                    list.RemoveRange(0, (int)unUseCount);
                }

                if (unUseCount == count)
                {
                    return unUses;
                }
            }

            var pages = PageManager.Allocate(token, (uint)(count - unUses.Length));
            if (pages == null)
            {
                throw new NullReferenceException(nameof(pages));
            }

            AllocationPages.AddRange(pages);
            return unUses.Concat(pages).ToArray();
        }

        public Page GetPage(PageIdentity identity)
        {
            if (ModificationPages.TryGetValue(identity, out var page))
            {
                return page;
            }

            if (AllocationPages.Contains(identity))
            {
                return CreatePage(identity);
            }

            var buffer = Pool.GetEntry(identity);
            if (buffer != null)
            {
                return buffer;
            }

            var fromDiskPage = PageManager.GetPage(identity);

            Pool.SetEntry(fromDiskPage, 0, PageEntryState.None);

            return fromDiskPage;
        }

        public Page ModifyPage(PageIdentity identity)
        {
            if (ModificationPages.TryGetValue(identity, out var page))
            {
                return page;
            }

            var oldPage = GetPage(identity);
            var newPage = ModifyPage(oldPage);

            if (!ModificationPages.TryAdd(identity, newPage))
            {
                throw new InvalidOperationException($"modify page {identity.PageNumber} failed!");
            }

            return newPage;
        }

        public void FreePage(PageIdentity identity)
        {
            UnUsingPages.AddOrUpdate(identity.Token, new HashSet<PageIdentity>() { identity }, (k, list) =>
            {
                list.Add(identity);
                return list;
            });
        }

        public void FreePage(IEnumerable<PageIdentity> identities)
        {
            foreach (var item in identities.GroupBy(i => i.Token))
            {
                UnUsingPages.AddOrUpdate(item.Key, new HashSet<PageIdentity>(item), (k, list) =>
                {
                    list.AddRange(item);
                    return list;
                });
            }
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
            ref var header = ref page.FileHeader;

            header.Identity.Token = identity.Token;
            header.Identity.PageNumber = identity.PageNumber;

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
}
