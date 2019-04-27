using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Vicuna.Storage.Paging.Impl
{
    public class PageBufferPool : IPageBufferPool
    {
        private readonly IPageManager _pageManager;

        private readonly ConcurrentDictionary<PageNumberInfo, PageEntry> _buffers;

        public virtual uint Limit { get; }

        public Action<PageNumberInfo, PageEntry> OnCleaning { get; set; }

        public PageBufferPool()
        {
            _buffers = new ConcurrentDictionary<PageNumberInfo, PageEntry>();
        }

        public virtual Page GetEntry(PageNumberInfo number)
        {
            if (_buffers.TryGetValue(number, out var bufferEntry))
            {
                Interlocked.Add(ref bufferEntry.Version, 1);
                return bufferEntry.Page;
            }

            return null;
        }

        public virtual void AddEntry(PageEntry entry)
        {
            _buffers.AddOrUpdate(new PageNumberInfo(entry.Page.PageHeader.PagerId, entry.Page.PageHeader.PageNumber), entry, (k, v) => entry);
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
