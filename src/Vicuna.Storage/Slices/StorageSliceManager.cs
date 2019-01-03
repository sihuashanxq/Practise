using System;
using Vicuna.Storage.Slices;
using Vicuna.Storage.Transactions;

namespace Vicuna.Storage
{
    public unsafe class StorageSliceManager
    {
        private readonly StorageLevelTransaction _tx;

        internal StorageSliceActivingList ActivedSlices => _tx.ActivedSlices;

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
            var pageNumber = _tx.AllocateSlicePage();
            var sliceHeadPage = _tx.GetPageToModify(pageNumber);
            if (sliceHeadPage == null)
            {
                throw new NullReferenceException(nameof(sliceHeadPage));
            }

            fixed (byte* buffer = sliceHeadPage.Buffer)
            {
                var header = (SlicePageHeader*)buffer;
                var dataPointer = (SpaceUsage*)&buffer[Constants.PageHeaderSize];
                var activingSliceSpaceEntry = new StorageSliceSpaceEntry()
                {
                    Usage = new SpaceUsage(pageNumber, Constants.StorageSliceDefaultUsedLength),
                    Index = -1,
                    OwnerOffset = -1
                };

                ActivedSlices.Insert(activingSliceSpaceEntry);

                header->FreeSize = 0;
                header->PageOffset = pageNumber;
                header->PrePageOffset = -1;
                header->NextPageOffset = -1;
                header->ItemCount = Constants.SlicePageCount;
                header->PageSize = Constants.PageSize;
                header->LastUsedOffset = Constants.PageSize - 1;
                header->ModifiedCount += Constants.PageSize;
                header->UsedLength = Constants.StorageSliceDefaultUsedLength;
                header->ActivedNodeIndex = activingSliceSpaceEntry.Index;
                header->ActivedNodeOffset = activingSliceSpaceEntry.OwnerOffset;

                //head page usage
                dataPointer->PageOffset = pageNumber;
                dataPointer->UsedLength = Constants.PageSize;
            }

            return new StorageSlice(_tx, sliceHeadPage);
        }
    }
}
