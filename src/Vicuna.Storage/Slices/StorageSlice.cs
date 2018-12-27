using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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

        internal StorageSliceActivePageEntry LastUsedPageEntry => _lastUsedPageEntry ?? (_lastUsedPageEntry = GetSliceUnUsedPage());

        internal StorageSiceHeadPage SliceHeadPage => _sliceHeadPage;

        public StorageSlice(StorageLevelTransaction tx, long sliceHeadPageOffset)
            : this(tx, tx.GetPageToModify(sliceHeadPageOffset))
        {

        }

        public StorageSlice(StorageLevelTransaction tx, byte[] headPageContent)
        {
            _tx = tx;
            _sliceHeadPage = new StorageSiceHeadPage(headPageContent);
        }

        public bool AllocatePage(out byte[] pageContent)
        {
            if (SliceHeadPage.FreePageIndexs.Count == 0)
            {
                pageContent = null;
                return false;
            }

            var firstFreePageIndex = SliceHeadPage.FreePageIndexs.First();
            if (firstFreePageIndex <= 0)
            {
                throw new IndexOutOfRangeException(nameof(firstFreePageIndex));
            }

            if ((pageContent = _tx.GetPageToModify(firstFreePageIndex + SliceHeadPage.PageOffset)) == null)
            {
                return false;
            }

            SliceHeadPage.SetPageEntry(firstFreePageIndex, Constants.PageHeaderSize, Constants.PageSize);
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

            foreach (var item in SliceHeadPage.ActivedPageMapping)
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
                return true;
            }

            return false;
        }

        public bool Allocate(StorageSliceActivePageEntry pageEntry, short size, out AllocationBuffer buffer)
        {
            var pageHeader = pageEntry.GetPageHeader();
            if (pageHeader.LastUsedOffset + size > Constants.PageSize)
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
                    UsedLength = Constants.PageSize - modifiedPageHead->FreeSize
                };

                if (modifiedPageHead->FreeSize - size <= 128)
                {
                    modifiedPageHead->FreeSize = 0;
                    modifiedPageHead->LastUsedOffset = Constants.PageSize - 1;
                    modifiedPageHead->ItemCount++;
                    modifiedPageHead->ModifiedCount += size;
                    modifiedPageHead->UsedLength = Constants.PageSize;

                    _lastUsedPageEntry = null;
                    SliceHeadPage.SetPageEntry(pageEntry.Index, pageHeader.UsedLength, modifiedPageHead->UsedLength);
                }
                else
                {
                    modifiedPageHead->FreeSize -= size;
                    modifiedPageHead->LastUsedOffset += size;
                    modifiedPageHead->ItemCount++;
                    modifiedPageHead->ModifiedCount += size;
                    modifiedPageHead->UsedLength += size;

                    _lastUsedPageEntry = pageEntry;
                    SliceHeadPage.SetPageEntry(pageEntry.Index, pageHeader.UsedLength, modifiedPageHead->UsedLength);
                }

                buffer = new AllocationBuffer(modifiedPage, pageHeader.LastUsedOffset, size);
                return true;
            }
        }

        protected StorageSliceActivePageEntry GetSliceUnUsedPage()
        {
            if (SliceHeadPage.FreePageCount == 0)
            {
                return null;
            }

            var firstFreePageIndex = SliceHeadPage.FreePageIndexs.First();
            if (firstFreePageIndex < 0)
            {
                throw new IndexOutOfRangeException(nameof(firstFreePageIndex));
            }

            var pageContent = _tx.GetPageToModify(firstFreePageIndex + _sliceHeadPage.PageOffset);
            if (pageContent == null)
            {
                return null;
            }

            return new StorageSliceActivePageEntry(firstFreePageIndex, pageContent);
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
