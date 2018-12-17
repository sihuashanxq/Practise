﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Vicuna.Storage.Pages;

namespace Vicuna.Storage
{
    public class StorageSlice : IDisposable
    {
        private Pager _pager;

        private Page _lastUsedPage;

        private StorageSpaceEntry _usage;

        private StoragePage _storageSlicePage;

        private readonly Queue<long> _freePages;

        private readonly HashSet<long> _fullPages;

        private readonly ConcurrentDictionary<long, Page> _cachedPages;

        private readonly ConcurrentDictionary<long, StorageSpaceEntry> _notFullPages;

        internal Page LastUsedPage => _lastUsedPage ?? (_lastUsedPage = GetSliceUnUsedPage());

        internal StorageSpaceEntry Usage => _usage;

        internal StoragePage StroageSlicePage => _storageSlicePage;

        public StorageSlice(StorageSlicePage stroageSlicePage, Pager pager)
        {
            _freePages = new Queue<long>();
            _fullPages = new HashSet<long>();
            _pager = pager ?? throw new NullReferenceException(nameof(pager));
            _storageSlicePage = stroageSlicePage ?? throw new NullReferenceException(nameof(_storageSlicePage));
            _notFullPages = new ConcurrentDictionary<long, StorageSpaceEntry>();
            _usage = new StorageSpaceEntry(stroageSlicePage.PagePos);
            _cachedPages = new ConcurrentDictionary<long, Page>() { [stroageSlicePage.PagePos] = stroageSlicePage };

            InitializeSliceUsage();
        }

        public bool AllocatePage(int pageCount, out Page[] pages)
        {
            if (_freePages.Count < pageCount)
            {
                pages = null;
                return false;
            }

            pages = new Page[pageCount];

            for (var i = pageCount - 1; i >= 0; i--)
            {
                var page = GetPage(_freePages.Dequeue());
                if (page == null)
                {
                    throw new NullReferenceException(nameof(page));
                }

                pages[i] = page;

                _fullPages.Add(page.PagePos);
                _usage.UsedSize += Constants.PageSize - Constants.PageHeaderSize;
            }

            return true;
        }

        public bool AllocatePage(out Page page)
        {
            if (_freePages.Count > 0)
            {
                page = GetPage(_freePages.Dequeue()) ?? throw new NullReferenceException(nameof(page));
                _fullPages.Add(page.PagePos);
                _usage.UsedSize += Constants.PageSize - Constants.PageHeaderSize;
                return true;
            }

            page = null;
            return false;
        }

        public bool Allocate(int size, out AllocationBuffer buffer)
        {
            if (size > Constants.PageSize - Constants.PageHeaderSize)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            if (Allocate(LastUsedPage, (short)size, out buffer))
            {
                return true;
            }

            foreach (var item in _notFullPages.Values.ToList())
            {
                var page = GetPage(item.Pos);
                if (page == null)
                {
                    continue;
                }

                if (Allocate(page, (short)size, out buffer))
                {
                    return true;
                }
            }

            var unUsedPage = GetSliceUnUsedPage();
            if (unUsedPage == null)
            {
                return false;
            }

            if (Allocate(unUsedPage, (short)size, out buffer))
            {
                _lastUsedPage = unUsedPage;
                return true;
            }

            return false;
        }

        public bool Allocate(Page page, short size, out AllocationBuffer buffer)
        {
            if (page == null || page.LastUsed + size > Constants.PageSize)
            {
                buffer = null;
                return false;
            }

            buffer = new AllocationBuffer(page, page.LastUsed, size);

            page.FreeSize -= size;
            page.LastUsed += size;
            page.ItemCount += 1;
            page.ModifiedCount += size;
            page.FlushPageHeader();

            if (page.FreeSize == 0)
            {
                _lastUsedPage = null;
                _lastUsedPage.LastUsed += size;
                _fullPages.Add(page.PagePos);
                _notFullPages.TryRemove(page.PagePos, out var _);
                return true;
            }

            var entry = new StorageSpaceEntry(page.PagePos, Constants.PageSize - page.FreeSize);

            _lastUsedPage = page;
            _usage.UsedSize += size;
            _notFullPages.AddOrUpdate(page.PagePos, entry, (k, v) => entry);
            return true;
        }

        protected Page GetSliceUnUsedPage()
        {
            if (_freePages.Count == 0)
            {
                return null;
            }

            return GetPage(_freePages.Dequeue());
        }

        protected Page GetPage(long pos)
        {
            if (_cachedPages.TryGetValue(pos, out var cachedPage))
            {
                return cachedPage;
            }

            var page = _pager.GetPage(pos);
            if (page != null)
            {
                _cachedPages.TryAdd(pos, page);
            }

            return page;
        }

        private void InitializeSliceUsage()
        {
            for (var i = 0; i < _storageSlicePage.ItemCount; i++)
            {
                var entryOffset = Constants.PageHeaderSize + i * StorageSliceSpaceEntry.SizeOf;
                var usageEntry = _storageSlicePage.GetEntry(entryOffset);
                if (usageEntry.UsedSize == Constants.PageSize)
                {
                    _fullPages.Add(usageEntry.Pos);
                    _usage.UsedSize += usageEntry.UsedSize;
                    continue;
                }

                if (usageEntry.UsedSize <= Constants.PageHeaderSize)
                {
                    _freePages.Enqueue(usageEntry.Pos);
                    _usage.UsedSize += usageEntry.UsedSize;
                    continue;
                }

                _notFullPages.TryAdd(usageEntry.Pos, usageEntry);
                _usage.UsedSize += usageEntry.UsedSize;
            }
        }

        public unsafe void Dispose()
        {
            var index = 0;
            var offset = Constants.PageHeaderSize;
            var slicePage = _storageSlicePage;

            foreach (var item in _fullPages)
            {
                offset += StorageSliceSpaceEntry.SizeOf;
                slicePage.SetEntry(offset, new StorageSpaceEntry(item, Constants.PageSize));
                index++;
            }

            foreach (var item in _notFullPages)
            {
                offset += StorageSliceSpaceEntry.SizeOf;
                slicePage.SetEntry(offset, item.Value);
                index++;
            }

            foreach (var item in _freePages)
            {
                offset += StorageSliceSpaceEntry.SizeOf;
                slicePage.SetEntry(offset, new StorageSpaceEntry(item));
                index++;
            }
        }
    }
}
