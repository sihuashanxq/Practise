using System;
using Vicuna.Storage.Pages;
using Vicuna.Storage.Transactions;

namespace Vicuna.Storage
{
    public unsafe class StorageSliceManager
    {
        private readonly StorageLevelTransaction _tx;

        public StorageSliceFreeHandling SliceFreeeHandling { get; }

        public const int PageCount = 1024;

        public StorageSliceManager(StorageLevelTransaction tx)
        {
            _tx = tx;
        }

        public StorageSlice GetSlice(long pageOffset)
        {
            return new StorageSlice(_tx, pageOffset);
        }

        public StorageSlice Allocate()
        {
            var slicePages = _tx.AllocatePageFromBuffer(PageCount);
            if (slicePages == null)
            {
                throw new NullReferenceException(nameof(slicePages));
            }

            var pageOffset = slicePages[0];
            var sliceHeadPage = _tx.GetPageToModify(pageOffset);
            if (sliceHeadPage == null)
            {
                throw new NullReferenceException(nameof(sliceHeadPage));
            }

            fixed (byte* buffer = sliceHeadPage)
            {
                var header = (PageHeader*)buffer;
                var entry = (StorageSliceSpaceUsage*)&buffer[Constants.PageHeaderSize];

                header->PageOffset = pageOffset;
                header->FreeSize = 0;
                header->PrePageOffset = -1;
                header->NextPageOffset = -1;
                header->ItemCount = PageCount;
                header->PageSize = Constants.PageSize;
                header->LastUsedOffset = Constants.PageSize - 1;
                header->ModifiedCount += Constants.PageSize;
                header->UsedLength = 1024 * 16 + 64 * 1023;

                //head page
                entry->PageOffset = pageOffset;
                entry->UsedLength = Constants.PageHeaderSize;
            }

            return new StorageSlice(_tx, sliceHeadPage);
        }
    }
}
