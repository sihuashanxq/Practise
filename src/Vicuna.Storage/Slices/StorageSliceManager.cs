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
            var headPageOffset = _tx.AllocateSlicePage();
            var headPage = _tx.GetPageToModify(headPageOffset);
            if (headPage == null)
            {
                throw new NullReferenceException(nameof(headPage));
            }

            fixed (byte* buffer = headPage.Buffer)
            {
                var header = (PageHeader*)buffer;
                var entry = (StorageSliceSpaceUsage*)&buffer[Constants.PageHeaderSize];

                header->PageOffset = headPageOffset;
                header->FreeSize = 0;
                header->PrePageOffset = -1;
                header->NextPageOffset = -1;
                header->ItemCount = PageCount;
                header->PageSize = Constants.PageSize;
                header->LastUsedOffset = Constants.PageSize - 1;
                header->ModifiedCount += Constants.PageSize;
                header->UsedLength = Constants.StorageSliceDefaultUsedLength;

                //head page
                entry->PageOffset = headPageOffset;
                entry->UsedLength = Constants.PageSize;
            }

            return new StorageSlice(_tx, headPage);
        }
    }
}
