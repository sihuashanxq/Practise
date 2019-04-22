using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Vicuna.Storage.Paging.Impl
{
    public class PageBufferPool : IPageBufferPool
    {
        private readonly IPageManager _pageManager;

        private readonly ConcurrentDictionary<PageIdentity, PageEntry> _buffers;

        public virtual uint Limit { get; }

        public Action<PageIdentity, PageEntry> OnCleaning { get; set; }

        public PageBufferPool()
        {
            _buffers = new ConcurrentDictionary<PageIdentity, PageEntry>();
        }

        public virtual Page GetEntry(PageIdentity identity)
        {
            if (_buffers.TryGetValue(identity, out var bufferEntry))
            {
                Interlocked.Add(ref bufferEntry.Version, 1);
                return bufferEntry.Page;
            }

            return null;
        }

        public virtual void AddEntry(PageEntry entry)
        {
            _buffers.AddOrUpdate(new PageIdentity(entry.Page.PageHeader.PagerId, entry.Page.PageHeader.PageNumber), entry, (k, v) => entry);
        }

        public void AddEntry(Page page, PageEntryState state)
        {
            AddEntry(new PageEntry(page, 0, state));
        }

        public void Dispose()
        {
            _buffers.Clear();
        }
    }
}
