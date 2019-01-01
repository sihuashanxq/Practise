using System;
using System.Linq;
using Vicuna.Storage.Data;
using Vicuna.Storage.Pages;
using Vicuna.Storage.Slices;
using Vicuna.Storage.Transactions;

namespace Vicuna.Storage
{
    public unsafe class StorageSlice
    {
        private StorageLevelTransaction _tx;

        private StorageSiceHeadPage _sliceHeadPage;

        private StorageSliceActivingPageEntry _activedPageEntry;

        internal StorageSiceHeadPage SliceHeadPage => _sliceHeadPage;

        internal StorageSliceActivingPageEntry ActivedPageEntry => _activedPageEntry ?? (_activedPageEntry = GetSliceUnUsedPage());

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

            if (Allocate(ActivedPageEntry, (short)size, out pageSlice))
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

                if (Allocate(new StorageSliceActivingPageEntry(item.Key, pageContent), (short)size, out pageSlice))
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

        private bool Allocate(StorageSliceActivingPageEntry pageEntry, short size, out PageSlice pageSlice)
        {
            return
                AllocateAtLastUsing(pageEntry, size, out pageSlice) ||
                AllocateAtHeadFreeList(pageEntry, size, out pageSlice);
        }

        private bool AllocateAtLastUsing(StorageSliceActivingPageEntry pageEntry, short size, out PageSlice pageSlice)
        {
            var pageHeader = pageEntry.GetPageHeader();
            if (pageHeader.LastUsedOffset + size > Constants.PageSize)
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
                if (modifiedPageHead->FreeSize - size <= 128)
                {
                    modifiedPageHead->FreeSize = 0;
                    modifiedPageHead->LastUsedOffset = Constants.PageSize - 1;
                    modifiedPageHead->ItemCount++;
                    modifiedPageHead->ModifiedCount += size;
                    modifiedPageHead->UsedLength = Constants.PageSize;

                    _activedPageEntry = null;
                    SliceHeadPage.SetPageEntry(pageEntry.Index, pageHeader.UsedLength, modifiedPageHead->UsedLength);
                }
                else
                {
                    modifiedPageHead->FreeSize -= size;
                    modifiedPageHead->LastUsedOffset += size;
                    modifiedPageHead->ItemCount++;
                    modifiedPageHead->ModifiedCount += size;
                    modifiedPageHead->UsedLength += size;

                    _activedPageEntry = new StorageSliceActivingPageEntry(pageEntry.Index, modifiedPage);
                    SliceHeadPage.SetPageEntry(pageEntry.Index, pageHeader.UsedLength, modifiedPageHead->UsedLength);
                }

                pageSlice = new PageSlice(modifiedPage, pageHeader.LastUsedOffset, size);
                return true;
            }
        }

        private bool AllocateAtHeadFreeList(StorageSliceActivingPageEntry pageEntry, short size, out PageSlice pageSlice)
        {
            var pageHeader = pageEntry.GetPageHeader();
            if (pageHeader.FreeEntryOffset == -1 ||
                pageHeader.FreeEntryLength < size)
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
                var freeDataEntry = *(FreeDataRecordEntry*)&pagePointer[modifiedPageHead->FreeEntryOffset];
                if (freeDataEntry.Next != -1)
                {
                    modifiedPageHead->FreeEntryOffset = freeDataEntry.Next;
                    modifiedPageHead->FreeEntryLength = ((FreeDataRecordEntry*)&pagePointer[freeDataEntry.Next])->Size;
                }
                else
                {
                    modifiedPageHead->FreeEntryOffset = -1;
                    modifiedPageHead->FreeEntryLength = -1;
                }

                if (modifiedPageHead->LastUsedOffset >= Constants.PageSize - 1 &&
                    modifiedPageHead->FreeEntryOffset == -1)
                {
                    //full
                    modifiedPageHead->ItemCount++;
                    modifiedPageHead->FreeSize = 0;
                    modifiedPageHead->UsedLength = Constants.PageSize;
                    modifiedPageHead->ModifiedCount += freeDataEntry.Size;

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

                pageSlice = new PageSlice(modifiedPage, pageHeader.FreeEntryOffset, size);
                return true;
            }
        }

        private StorageSliceActivingPageEntry GetSliceUnUsedPage()
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

            return new StorageSliceActivingPageEntry(freePageIndex, page);
        }
    }
}
