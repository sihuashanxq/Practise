using System;
using System.Linq;
using Vicuna.Storage.Pages;
using Vicuna.Storage.Slices;
using Vicuna.Storage.Transactions;

namespace Vicuna.Storage
{
    public unsafe class StorageSlice
    {
        private StorageLevelTransaction _tx;

        private StorageSiceHeadPage _sliceHeadPage;

        private StorageSliceActivePageEntry _lastUsedPageEntry;

        internal StorageSiceHeadPage SliceHeadPage => _sliceHeadPage;

        internal StorageSliceActivePageEntry LastUsedPageEntry => _lastUsedPageEntry ?? (_lastUsedPageEntry = GetSliceUnUsedPage());

        public StorageSlice(StorageLevelTransaction tx, long sliceHeadPageOffset)
        {
            _tx = tx;
            _sliceHeadPage = new StorageSiceHeadPage(_tx.GetPageToModify(sliceHeadPageOffset).Buffer);
        }

        public StorageSlice(StorageLevelTransaction tx, Page headPage)
        {
            _tx = tx;
            _sliceHeadPage = new StorageSiceHeadPage(headPage.Buffer);
        }

        public bool AllocatePage(out Page page)
        {
            if (SliceHeadPage.FreePageIndexs.Count == 0)
            {
                page = null;
                return false;
            }

            var freePageIndex = SliceHeadPage.FreePageIndexs.First();
            if (freePageIndex <= 0)
            {
                throw new IndexOutOfRangeException(nameof(freePageIndex));
            }

            if ((page = _tx.GetPageToModify(freePageIndex + SliceHeadPage.PageOffset)) == null)
            {
                return false;
            }

            SliceHeadPage.SetPageEntry(freePageIndex, Constants.PageHeaderSize, Constants.PageSize);
            return true;
        }

        public bool Allocate(int size, out PageSlice pageSlice)
        {
            if (size > Constants.PageSize - Constants.PageHeaderSize)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            if (Allocate(LastUsedPageEntry, (short)size, out pageSlice))
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

                if (Allocate(new StorageSliceActivePageEntry(item.Key, pageContent), (short)size, out pageSlice))
                {
                    return true;
                }
            }

            var unUsedPage = GetSliceUnUsedPage();
            if (unUsedPage == null)
            {
                return false;
            }

            if (Allocate(unUsedPage, (short)size, out pageSlice))
            {
                return true;
            }

            return false;
        }

        private bool Allocate(StorageSliceActivePageEntry pageEntry, short size, out PageSlice pageSlice)
        {
            var pageHeader = pageEntry.GetPageHeader();
            if (pageHeader.UsedLength + size > Constants.PageSize)
            {
                pageSlice = null;
                return false;
            }

            var modifiedPage = _tx.GetPageToModify(pageHeader.PageOffset);
            if (modifiedPage == null)
            {
                throw new NullReferenceException(nameof(modifiedPage));
            }

            fixed (byte* pagePointer = modifiedPage.Buffer)
            {
                var modifiedPageHead = (PageHeader*)pagePointer;
                var spaceEntry = new StorageSliceSpaceUsage()
                {
                    PageOffset = pageHeader.PageOffset,
                    UsedLength = modifiedPageHead->UsedLength
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

                    _lastUsedPageEntry = new StorageSliceActivePageEntry(pageEntry.Index, modifiedPage);
                    SliceHeadPage.SetPageEntry(pageEntry.Index, pageHeader.UsedLength, modifiedPageHead->UsedLength);
                }

                pageSlice = new PageSlice(modifiedPage, pageHeader.LastUsedOffset, size);
                return true;
            }
        }

        private StorageSliceActivePageEntry GetSliceUnUsedPage()
        {
            if (SliceHeadPage.FreePageCount == 0)
            {
                return null;
            }

            var freePageIndex = SliceHeadPage.FreePageIndexs.First();
            if (freePageIndex < 0)
            {
                throw new IndexOutOfRangeException(nameof(freePageIndex));
            }

            var page = _tx.GetPage(freePageIndex + _sliceHeadPage.PageOffset);
            if (page == null)
            {
                return null;
            }

            return new StorageSliceActivePageEntry(freePageIndex, page);
        }
    }
}
