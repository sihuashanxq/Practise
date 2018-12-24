using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Vicuna.Storage.Pages;
using Vicuna.Storage.Transactions;

namespace Vicuna.Storage
{
    public unsafe class StorageSlice : IDisposable
    {
        private byte[] _lastUsedPage;

        private StorageSpaceEntry _usage;

        private StoragePage _storageSlicePage;

        private readonly Queue<long> _freePages;

        private readonly HashSet<long> _fullPages;

        private readonly StorageLevelTransaction _tx;

        private readonly ConcurrentDictionary<long, StorageSpaceEntry> _notFullPages;

        internal byte[] LastUsedPage => _lastUsedPage ?? (_lastUsedPage = GetSliceUnUsedPage());

        internal StorageSpaceEntry Usage => _usage;

        internal StoragePage StroageSlicePage => _storageSlicePage;

        public StorageSlice(StorageLevelTransaction tx, StorageSlicePage storageSlicePage)
        {
            _tx = tx;
            _freePages = new Queue<long>();
            _fullPages = new HashSet<long>();
            _storageSlicePage = storageSlicePage;
            _notFullPages = new ConcurrentDictionary<long, StorageSpaceEntry>();
            _usage = new StorageSpaceEntry(storageSlicePage.PagePos);
            InitializeSlicePageEntries();
        }

        public bool AllocatePage(out byte[] allocatedPage)
        {
            if (_freePages.Count == 0)
            {
                allocatedPage = null;
                return false;
            }

            var pageOffset = _freePages.Dequeue();
            var page = _tx.GetPageToModify(_freePages.Dequeue());

            _fullPages.Add(pageOffset);
            _usage.UsedSize += Constants.PageSize - Constants.PageHeaderSize;
            allocatedPage = page;
            return true;
        }

        public bool AllocatePage(int pageCount, List<byte[]> allocatedPages)
        {
            if (_freePages.Count < pageCount)
            {
                return false;
            }

            for (var i = 0; i < pageCount; i++)
            {
                if (AllocatePage(out var page))
                {
                    allocatedPages.Add(page);
                }

                throw new InvalidOperationException("allocate page faild!");
            }

            return true;
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
                var pageContent = _tx.GetPage(item.Pos);
                if (pageContent == null)
                {
                    continue;
                }

                if (Allocate(pageContent, (short)size, out buffer))
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

        public bool Allocate(byte[] pageContent, short size, out AllocationBuffer buffer)
        {
            var pageHead = GetPageHeader(pageContent);
            if (pageHead.LastUsedPos + size > Constants.PageSize)
            {
                buffer = null;
                return false;
            }

            var modifiedPage = _tx.GetPageToModify(pageHead.PageOffset);
            if (modifiedPage == null)
            {
                throw new NullReferenceException(nameof(modifiedPage));
            }

            fixed (byte* pagePointer = modifiedPage)
            {
                var modifiedPageHead = (PageHeader*)pagePointer;
                var spaceEntry = new StorageSpaceEntry(modifiedPageHead->PageOffset, Constants.PageSize - modifiedPageHead->FreeSize);
                if (modifiedPageHead->FreeSize - size <= 128)
                {
                    modifiedPageHead->FreeSize = 0;
                    modifiedPageHead->LastUsedPos = Constants.PageSize - 1;
                    modifiedPageHead->ItemCount++;
                    modifiedPageHead->ModifiedCount += size;

                    _lastUsedPage = null;
                    _fullPages.Add(modifiedPageHead->PageOffset);
                    _notFullPages.TryRemove(modifiedPageHead->PageOffset, out var _);
                }
                else
                {
                    modifiedPageHead->FreeSize -= size;
                    modifiedPageHead->LastUsedPos += size;
                    modifiedPageHead->ItemCount++;
                    modifiedPageHead->ModifiedCount += size;

                    _lastUsedPage = null;
                    _usage.UsedSize += size;
                    _notFullPages.AddOrUpdate(modifiedPageHead->PageOffset, spaceEntry, (k, v) => spaceEntry);
                }

                buffer = new AllocationBuffer(modifiedPage, pageHead.LastUsedPos, size);
                return true;
            }
        }

        protected byte[] GetSliceUnUsedPage()
        {
            if (_freePages.Count == 0)
            {
                return null;
            }

            var pageOffset = _freePages.Dequeue();
            var pageContent = _tx.GetPage(pageOffset);
            if (pageContent != null)
            {
                _notFullPages.TryAdd(pageOffset, new StorageSpaceEntry(pageOffset, Constants.PageHeaderSize));
                return pageContent;
            }

            return null;
        }

        public PageHeader GetPageHeader(byte[] pageContent)
        {
            fixed (byte* pagePointer = pageContent)
            {
                return *(PageHeader*)pagePointer;
            }
        }

        private void InitializeSlicePageEntries()
        {
            for (var i = 0; i < 1024; i++)
            {
                var entryOffset = Constants.PageHeaderSize + i * StorageSliceSpaceUsage.SizeOf;
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
                    _usage.UsedSize += Constants.PageHeaderSize;
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
                offset += StorageSliceSpaceUsage.SizeOf;
                slicePage.SetEntry(offset, new StorageSpaceEntry(item, Constants.PageSize));
                index++;
            }

            foreach (var item in _notFullPages)
            {
                offset += StorageSliceSpaceUsage.SizeOf;
                slicePage.SetEntry(offset, item.Value);
                index++;
            }

            foreach (var item in _freePages)
            {
                offset += StorageSliceSpaceUsage.SizeOf;
                slicePage.SetEntry(offset, new StorageSpaceEntry(item));
                index++;
            }
        }
    }
}
