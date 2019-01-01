using System;
using Vicuna.Storage.Slices;
using Vicuna.Storage.Transactions;

namespace Vicuna.Storage
{
    public unsafe class StorageSliceManager
    {
        private readonly StorageLevelTransaction _tx;

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
                var header = (SlicePageHeader*)buffer;
                var usage = (SpaceUsage*)&buffer[Constants.PageHeaderSize];
                var spaceEntry = _tx.ActivedSlices.Insert(headPageOffset, Constants.StorageSliceDefaultUsedLength);

                header->FreeSize = 0;
                header->PageOffset = headPageOffset;
                header->PrePageOffset = -1;
                header->NextPageOffset = -1;
                header->ItemCount = Constants.SlicePageCount;
                header->PageSize = Constants.PageSize;
                header->LastUsedOffset = Constants.PageSize - 1;
                header->ModifiedCount += Constants.PageSize;
                header->UsedLength = Constants.StorageSliceDefaultUsedLength;
                header->ActivedNodeIndex = spaceEntry.Index;
                header->ActivedNodeOffset = spaceEntry.OwnerOffset;

                //head page
                usage->PageOffset = headPageOffset;
                usage->UsedLength = Constants.PageSize;
            }

            return new StorageSlice(_tx, headPage);
        }
    }
}
