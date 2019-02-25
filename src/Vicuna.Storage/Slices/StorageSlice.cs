using System;
using System.Linq;
using Vicuna.Storage.Data;
using Vicuna.Storage.Pages;
using Vicuna.Storage.Slices;
using Vicuna.Storage.Transactions;

namespace Vicuna.Storage
{
    public unsafe class StorageSlice : IDisposable
    {
        private StorageLevelTransaction _tx;

        private StorageSiceHeadPage _sliceHeadPage;

        private StorageSliceActivingPageEntry _activedPageEntry;

        internal StorageSiceHeadPage SliceHeadPage => _sliceHeadPage;

        internal StorageSliceActivingPageEntry ActivedPageEntry => _activedPageEntry ?? (_activedPageEntry = GetSliceUnUsedPage());

        public StorageSlice(StorageLevelTransaction tx, long pageNumber)
        {
            _tx = tx;
            _sliceHeadPage = new StorageSiceHeadPage(_tx.GetPageToModify(pageNumber));
        }

        public StorageSlice(StorageLevelTransaction tx, StorageSiceHeadPage sliceHeadPage)
        {
            _tx = tx;
            _sliceHeadPage = sliceHeadPage;
        }

        public bool AllocatePage(out Page page)
        {
            if (SliceHeadPage.FreePages.Count == 0)
            {
                page = null;
                return false;
            }

            var pageIndex = SliceHeadPage.FreePages.First();
            if (pageIndex <= 0)
            {
                throw new IndexOutOfRangeException(nameof(pageIndex));
            }

            if ((page = _tx.GetPageToModify(pageIndex + SliceHeadPage.PageNumber)) == null)
            {
                return false;
            }

            SliceHeadPage.SetPageEntry(pageIndex, Constants.PageHeaderSize, Constants.PageSize);
            return true;
        }

        public bool Allocate(int size, out PageSlice pageSlice)
        {
            if (size > Constants.PageSize - Constants.PageHeaderSize)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            if (Allocate(ActivedPageEntry, (short)size, out pageSlice))
            {
                return true;
            }

            foreach (var item in SliceHeadPage.ActivedPageMapping)
            {
                var page = _tx.GetPage(item.Value.PageNumber);
                if (page == null)
                {
                    continue;
                }

                if (Allocate(new StorageSliceActivingPageEntry(item.Key, page), (short)size, out pageSlice))
                {
                    return true;
                }
            }

            return Allocate(GetSliceUnUsedPage(), (short)size, out pageSlice);
        }

        private bool Allocate(StorageSliceActivingPageEntry pageEntry, short size, out PageSlice pageSlice)
        {
            return
                AllocateAtLastUsing(pageEntry, size, out pageSlice) ||
                AllocateAtHeadFreeList(pageEntry, size, out pageSlice);
        }

        private bool AllocateAtLastUsing(StorageSliceActivingPageEntry pageEntry, short size, out PageSlice pageSlice)
        {
            if (pageEntry == null)
            {
                pageSlice = null;
                return false;
            }

            var pageHeader = pageEntry.GetPageHeader();
            if (pageHeader.LastUsedIndex + size > Constants.PageSize)
            {
                pageSlice = null;
                return false;
            }

            var modifiedPage = _tx.GetPageToModify(pageHeader.PageNumber);
            if (modifiedPage == null)
            {
                throw new NullReferenceException(nameof(modifiedPage));
            }

            fixed (byte* pagePointer = modifiedPage.Buffer)
            {
                var modifiedPageHead = (PageHeader*)pagePointer;
                if (modifiedPageHead->FreeSize - size <= 128)
                {
                    modifiedPageHead->FreeSize = 0;
                    modifiedPageHead->LastUsedIndex = Constants.PageSize - 1;
                    modifiedPageHead->ItemCount++;
                    modifiedPageHead->ModifiedCount += size;
                    modifiedPageHead->UsedLength = Constants.PageSize;

                    _activedPageEntry = null;
                    SliceHeadPage.SetPageEntry(pageEntry.Index, pageHeader.UsedLength, modifiedPageHead->UsedLength);
                }
                else
                {
                    modifiedPageHead->FreeSize -= size;
                    modifiedPageHead->LastUsedIndex += size;
                    modifiedPageHead->ItemCount++;
                    modifiedPageHead->ModifiedCount += size;
                    modifiedPageHead->UsedLength += size;

                    _activedPageEntry = new StorageSliceActivingPageEntry(pageEntry.Index, modifiedPage);
                    SliceHeadPage.SetPageEntry(pageEntry.Index, pageHeader.UsedLength, modifiedPageHead->UsedLength);
                }

                pageSlice = new PageSlice(modifiedPage, pageHeader.LastUsedIndex, size);
                return true;
            }
        }

        private bool AllocateAtHeadFreeList(StorageSliceActivingPageEntry pageEntry, short size, out PageSlice pageSlice)
        {
            if (pageEntry == null)
            {
                pageSlice = null;
                return false;
            }

            var pageHeader = pageEntry.GetPageHeader();
            if (pageHeader.FreeEntryIndex == -1 ||
                pageHeader.FreeEntryLength < size)
            {
                pageSlice = null;
                return false;
            }

            var modifiedPage = _tx.GetPageToModify(pageHeader.PageNumber);
            if (modifiedPage == null)
            {
                throw new NullReferenceException(nameof(modifiedPage));
            }

            fixed (byte* pagePointer = modifiedPage.Buffer)
            {
                var modifiedPageHead = (PageHeader*)pagePointer;
                if (modifiedPageHead->LastUsedIndex >= Constants.PageSize - 1 &&
                    modifiedPageHead->FreeEntryIndex == -1)
                {
                    //full
                    modifiedPageHead->ItemCount++;
                    modifiedPageHead->FreeSize = 0;
                    modifiedPageHead->UsedLength = Constants.PageSize;

                    _activedPageEntry = null;
                    SliceHeadPage.SetPageEntry(pageEntry.Index, pageHeader.UsedLength, modifiedPageHead->UsedLength);
                }
                else
                {
                    modifiedPageHead->ItemCount++;
                    modifiedPageHead->FreeSize -= size;
                    modifiedPageHead->UsedLength += size;
                    modifiedPageHead->ModifiedCount += size;

                    _activedPageEntry = new StorageSliceActivingPageEntry(pageEntry.Index, modifiedPage);
                    SliceHeadPage.SetPageEntry(pageEntry.Index, pageHeader.UsedLength, modifiedPageHead->UsedLength);
                }

                pageSlice = new PageSlice(modifiedPage, pageHeader.FreeEntryIndex, size);
                return true;
            }
        }

        private StorageSliceActivingPageEntry GetSliceUnUsedPage()
        {
            if (SliceHeadPage.FreePageCount == 0)
            {
                return null;
            }

            var index = SliceHeadPage.FreePages.First();
            if (index < 0)
            {
                throw new IndexOutOfRangeException(nameof(index));
            }

            var page = _tx.GetPage(index + _sliceHeadPage.PageNumber);
            if (page == null)
            {
                return null;
            }

            return new StorageSliceActivingPageEntry(index, page);
        }

        public void Dispose()
        {
            if (SliceHeadPage.UsedLength == Constants.StorageSliceSize)
            {
                if (SliceHeadPage.ActivedNodePageNumber != -1)
                {
                    _tx.ActivedSlices.Delete(this);
                }
            }
            else if (SliceHeadPage.ActivedNodePageNumber == -1)
            {
                _tx.ActivedSlices.Insert(this);
            }
            else
            {
                _tx.ActivedSlices.Update(this);
            }
        }
    }
}
