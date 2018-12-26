using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Vicuna.Storage.Pages;
using Vicuna.Storage.Transactions;

namespace Vicuna.Storage
{
    public unsafe class StorageSlice
    {
        private StorageSliceActivePageEntry _lastUsedPageEntry;

        private StorageSpaceEntry _usage;

        private StorageSiceHeadPage _sliceHeadPage;

        private readonly StorageLevelTransaction _tx;

        private readonly Queue<int> _freePageIndexs;

        private readonly HashSet<int> _fullPageIndexs;

        private readonly ConcurrentDictionary<int, StorageSliceSpaceUsage> _activePageIndexMapping;

        internal StorageSliceActivePageEntry LastUsedPageEntry => _lastUsedPageEntry ?? (_lastUsedPageEntry = GetSliceUnUsedPage());

        internal StorageSiceHeadPage SliceHeadPage => _sliceHeadPage;

        public StorageSlice(StorageLevelTransaction tx, long sliceHeadPageOffset)
            :this(tx,tx.GetPageToModify(sliceHeadPageOffset))
        {

        }

        public StorageSlice(StorageLevelTransaction tx, byte[] headPageContent)
        {
            _tx = tx;
            _freePageIndexs = new Queue<int>();
            _fullPageIndexs = new HashSet<int>();
            _activePageIndexMapping = new ConcurrentDictionary<int, StorageSliceSpaceUsage>();
            _sliceHeadPage = new StorageSiceHeadPage(headPageContent);
            BuildPageUsageEntries();
        }

        private void BuildPageUsageEntries()
        {
            var pageEntries = _sliceHeadPage.GetPageEntries();
            if (pageEntries.Count == 0)
            {
                throw new InvalidOperationException("slice page count error!");
            }

            for (var i = 0; i < pageEntries.Count; i++)
            {
                var pageEntry = pageEntries[i];
                if (pageEntry.Usage.UsedLength == Constants.PageSize)
                {
                    _fullPageIndexs.Add(pageEntry.Index);
                    _usage.UsedSize += pageEntry.Usage.UsedLength;
                    continue;
                }

                if (pageEntry.Usage.UsedLength <= Constants.PageHeaderSize)
                {
                    _freePageIndexs.Enqueue(pageEntry.Index);
                    _usage.UsedSize += Constants.PageHeaderSize;
                    continue;
                }

                _activePageIndexMapping.TryAdd(pageEntry.Index, pageEntry.Usage);
                _usage.UsedSize += pageEntry.Usage.UsedLength;
            }
        }

        public bool AllocatePage(out byte[] page)
        {
            if (_freePageIndexs.Count == 0)
            {
                page = null;
                return false;
            }

            var pageOffset = _freePageIndexs.Dequeue();
            page = _tx.GetPageToModify(pageOffset);

            _usage.UsedSize += Constants.PageSize - Constants.PageHeaderSize;
            _fullPageIndexs.Add(pageOffset);

            return true;
        }

        public bool AllocatePage(int pageCount, List<byte[]> allocatedPages)
        {
            if (_freePageIndexs.Count < pageCount)
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

            if (Allocate(LastUsedPageEntry, (short)size, out buffer))
            {
                return true;
            }

            foreach (var item in _activePageIndexMapping)
            {
                var pageContent = _tx.GetPage(item.Value.PageOffset);
                if (pageContent == null)
                {
                    continue;
                }

                if (Allocate(new StorageSliceActivePageEntry(item.Key, pageContent), (short)size, out buffer))
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
                _lastUsedPageEntry = unUsedPage;
                return true;
            }

            return false;
        }

        public bool Allocate(StorageSliceActivePageEntry pageEntry, short size, out AllocationBuffer buffer)
        {
            var pageHeader = pageEntry.GetPageHeader();
            if (pageHeader.LastUsedPos + size > Constants.PageSize)
            {
                buffer = null;
                return false;
            }

            var modifiedPage = _tx.GetPageToModify(pageHeader.PageOffset);
            if (modifiedPage == null)
            {
                throw new NullReferenceException(nameof(modifiedPage));
            }

            fixed (byte* pagePointer = modifiedPage)
            {
                var modifiedPageHead = (PageHeader*)pagePointer;
                var spaceEntry = new StorageSliceSpaceUsage()
                {
                    PageOffset = pageHeader.PageOffset,
                    UsedLength = (uint)(Constants.PageSize - modifiedPageHead->FreeSize)
                };

                if (modifiedPageHead->FreeSize - size <= 128)
                {
                    modifiedPageHead->FreeSize = 0;
                    modifiedPageHead->LastUsedPos = Constants.PageSize - 1;
                    modifiedPageHead->ItemCount++;
                    modifiedPageHead->ModifiedCount += size;

                    _lastUsedPageEntry = null;
                    _fullPageIndexs.Add(pageEntry.Index);
                    _activePageIndexMapping.TryRemove(pageEntry.Index, out var _);
                }
                else
                {
                    modifiedPageHead->FreeSize -= size;
                    modifiedPageHead->LastUsedPos += size;
                    modifiedPageHead->ItemCount++;
                    modifiedPageHead->ModifiedCount += size;

                    _lastUsedPageEntry = null;
                    _usage.UsedSize += size;
                    _activePageIndexMapping.AddOrUpdate(pageEntry.Index, spaceEntry, (k, v) => spaceEntry);
                }

                buffer = new AllocationBuffer(modifiedPage, pageHeader.LastUsedPos, size);
                return true;
            }
        }

        protected StorageSliceActivePageEntry GetSliceUnUsedPage()
        {
            if (_freePageIndexs.Count == 0)
            {
                return null;
            }

            var pageIndex = _freePageIndexs.Dequeue();
            if (pageIndex < 0)
            {
                throw new IndexOutOfRangeException(nameof(pageIndex));
            }

            var pageOffset = pageIndex + _sliceHeadPage.PagePos;
            if (pageOffset < 0)
            {
                throw new IndexOutOfRangeException(nameof(pageOffset));
            }

            var pageContent = _tx.GetPage(pageIndex + _sliceHeadPage.PagePos);
            if (pageContent == null)
            {
                return null;
            }

            var pageEntry = new StorageSliceActivePageEntry(pageIndex, pageContent);
            var spaceUsage = new StorageSliceSpaceUsage(pageOffset, (uint)Constants.PageHeaderSize);

            _activePageIndexMapping.AddOrUpdate(pageIndex, spaceUsage, (k, v) => spaceUsage);

            return pageEntry;
        }
    }

    public unsafe class StorageSliceActivePageEntry
    {
        public int Index { get; set; }

        public byte[] PageContent { get; set; }

        public StorageSliceActivePageEntry(int index, byte[] pageContent)
        {
            Index = index;
            PageContent = pageContent;
        }

        public PageHeader GetPageHeader()
        {
            fixed (byte* pagePointer = PageContent)
            {
                return *(PageHeader*)pagePointer;
            }
        }
    }
}
