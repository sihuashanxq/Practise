using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Vicuna.Storage.Paging.Impl
{
    public class VicunaPageBufferBool : IPageBufferPool
    {
        public virtual uint Limit { get; }

        protected internal Action<PageIdentity, PageEntry> OnCleaning { get; set; }

        private readonly ConcurrentDictionary<PageIdentity, PageEntry> _buffers;

        public VicunaPageBufferBool()
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

        public virtual void SetEntry(PageEntry bufferEntry)
        {
            _buffers.AddOrUpdate(bufferEntry.Page.FileHeader.Identity, bufferEntry, (k, v) => bufferEntry);
        }

        public void SetEntry(Page page, int version, PageEntryState state)
        {
            SetEntry(new PageEntry(page, version, state));
        }

        public void Dispose()
        {
            _buffers.Clear();
        }
    }
}
