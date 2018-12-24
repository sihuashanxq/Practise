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
            var buffer = _tx.GetPageToModify(pageOffset);
            if (buffer == null)
            {
                throw new NullReferenceException(nameof(buffer));
            }

            return new StorageSlice(_tx, new StorageSlicePage(buffer));
        }

        public StorageSlice Allocate()
        {
            var slicePages = _tx.AllocatePageFromBuffer(PageCount);
            if (slicePages == null)
            {
                throw new NullReferenceException(nameof(slicePages));
            }

            var sliceHeadPageOffset = slicePages[0];
            var sliceHeadPage = _tx.GetPageToModify(sliceHeadPageOffset);
            if (sliceHeadPage == null)
            {
                throw new NullReferenceException(nameof(sliceHeadPage));
            }

            //初始化新分配的slice页
            fixed (byte* buffer = sliceHeadPage)
            {
                var header = (PageHeader*)buffer;
                var entry = (StorageSliceSpaceUsage*)&buffer[Constants.PageHeaderSize];

                header->PageOffset = sliceHeadPageOffset;
                header->FreeSize = 0;
                header->PrePageOffset = -1;
                header->NextPageOffset = -1;
                header->ItemCount = PageCount;
                header->PageSize = Constants.PageSize;
                header->LastUsedPos = Constants.PageSize - 1;
                header->ModifiedCount += Constants.PageSize;

                for (var i = 0; i < PageCount; i++)
                {
                    entry[i].PageOffset = sliceHeadPageOffset + i;
                    entry[i].UsedLength = i == 0 ? Constants.PageSize : Constants.PageHeaderSize;
                }
            }

            return new StorageSlice(_tx, new StorageSlicePage(sliceHeadPage));
        }
    }
}
