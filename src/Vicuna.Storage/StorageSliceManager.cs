using System;
using Vicuna.Storage.Pages;
using Vicuna.Storage.Transactions;

namespace Vicuna.Storage
{
    public class StorageSliceManager
    {
        private readonly StorageLevelTransaction _tx;

        public StorageSliceFreeHandling SliceFreeeHandling { get; }

        public const int SlicePageCount = 1024;

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

        public unsafe StorageSlice Allocate()
        {
            var slicePages = _tx.AllocatePage(SlicePageCount);
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
                var entry = (StorageSliceSpaceEntry*)&buffer[Constants.PageHeaderSize];

                header->PagePos = sliceHeadPageOffset;
                header->FreeSize = 0;
                header->PrePagePos = -1;
                header->NextPagePos = -1;
                header->ItemCount = SlicePageCount;
                header->PageSize = Constants.PageSize;
                header->LastUsedPos = Constants.PageSize - 1;
                header->ModifiedCount += Constants.PageSize;

                for (var i = 0; i < SlicePageCount; i++)
                {
                    entry[i].Pos = sliceHeadPageOffset + i;
                    entry[i].UsedSize = i == 0 ? Constants.PageSize : Constants.PageHeaderSize;
                }
            }

            return new StorageSlice(_tx, new StorageSlicePage(sliceHeadPage));
        }
    }
}
