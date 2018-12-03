using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Vicuna.Storage.Pages;

namespace Vicuna.Storage
{
    public class StorageSlice
    {
        public long Loc { get; }

        private Page _lastUsedPage;

        public StoragePage _slicePage;

        private readonly Stack<long> _freePages;

        private readonly HashSet<long> _fullPages;

        private readonly ConcurrentDictionary<long, StorageSpaceUsageEntry> _notFullPages;

        private Pager _pager;

        internal Page LastUsedPage
        {
            get
            {
                if (_lastUsedPage == null)
                {
                    _lastUsedPage = _pager.Create();
                }

                return _lastUsedPage;
            }
        }

        internal StorageSpaceUsageEntry Usage { get; }

        public StorageSlice(StorageSlicePage page, Pager pager)
        {
            _pager = pager;
            _slicePage = page;
            _freePages = new Stack<long>();
            _fullPages = new HashSet<long>();
            _notFullPages = new ConcurrentDictionary<long, StorageSpaceUsageEntry>();

            Initialize();
        }

        protected unsafe virtual void Initialize()
        {
            if (_slicePage == null)
            {
                throw new NullReferenceException(nameof(_slicePage));
            }

            for (var i = 0; i < _slicePage.ItemCount; i++)
            {
                var entry = _slicePage.GetEntry(Constants.PageHeaderSize + i * sizeof(StorageSpaceUsageEntry));
                if (entry.UsedSize <= Constants.PageHeaderSize)
                {
                    _freePages.Push(entry.Pos);
                    continue;
                }

                if (entry.UsedSize == Constants.PageSize)
                {
                    _fullPages.Add(entry.Pos);
                    continue;
                }

                _notFullPages.TryAdd(entry.Pos, entry);
            }
        }

        public bool Allocate(int size, out AllocationBuffer buffer)
        {
            if (Allocate(LastUsedPage, size, out buffer))
            {
                return true;
            }

            foreach (var item in _notFullPages.Values.ToList())
            {
                if (Allocate(item.Pos, size, out buffer))
                {
                    return true;
                }
            }

            buffer = null;
            return false;
        }

        private bool Allocate(long pos, int size, out AllocationBuffer buffer)
        {
            var page = GetPage(pos + Loc);
            if (page == null)
            {
                buffer = null;
                return false;
            }

            return Allocate(page, size, out buffer);
        }

        private bool Allocate(Page page, int size, out AllocationBuffer buffer)
        {
            if (page == null)
            {
                buffer = null;
                return false;
            }

            if (page.LastUsed + size > Constants.PageSize)
            {
                buffer = null;
                return false;
            }

            buffer = new AllocationBuffer(page, page.LastUsed, (short)size);

            page.ItemCount++;
            page.LastUsed += (short)size;
            page.FreeSize -= (short)size;

            if (page.FreeSize == 0)
            {
                _fullPages.Add(page.PageId);
            }
            else
            {
                _notFullPages.TryAdd(page.PageId, new StorageSpaceUsageEntry(page.PageId, Constants.PageSize - page.FreeSize));
            }

            return true;
        }

        private Page GetPage(long loc)
        {
            return _pager.GetPage(loc);
        }
    }
}
