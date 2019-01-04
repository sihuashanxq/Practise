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

        public StorageSlice CreateSlice(int minPageCount = 1)
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
                var dataPointer = (StorageSlicePageUsage*)&buffer[Constants.PageHeaderSize];

                header->FreeSize = 0;
                header->PageNumber = pageNumber;
                header->PrePageNumber = -1;
                header->NextPageNumber = -1;
                header->ItemCount = Constants.SlicePageCount;
                header->PageSize = Constants.PageSize;
                header->LastUsedIndex = Constants.PageSize - 1;
                header->ModifiedCount += Constants.PageSize;
                header->UsedLength = Constants.StorageSliceDefaultUsedLength;
                header->AcitvedNodeIndex = -1;
                header->AcitvedNodePageNumber = -1;

                for (var i = 0; i < header->ItemCount; i++)
                {
                    if (i == 0)
                    {
                        //head page usage
                        dataPointer[i].PageNumber = pageNumber + i;
                        dataPointer[i].UsedLength = Constants.PageSize;
                    }
                    else
                    {
                        dataPointer[i].PageNumber = pageNumber + i;
                        dataPointer[i].UsedLength = Constants.PageHeaderSize;
                    }
                }
            }

            return new StorageSlice(_tx, new StorageSiceHeadPage(sliceHeadPage.Buffer));
        }

        public StorageSlice GetSlice(long pageNumber)
        {
            return new StorageSlice(_tx, pageNumber);
        }
    }
}
