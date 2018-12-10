using System;
using Vicuna.Storage.Pages;

namespace Vicuna.Storage
{
    public class StorageSliceHandling
    {
        private Pager _pager;

        public const int SlicePageCount = 1024;

        public StorageSliceHandling(Pager pager)
        {
            _pager = pager;
        }

        public StorageSlice GetSlice(long slicePagePos)
        {
            var buffer = _pager.GetBuffer(slicePagePos);
            if (buffer == null)
            {
                throw new NullReferenceException(nameof(buffer));
            }

            return new StorageSlice(new StorageSlicePage(buffer), _pager);
        }

        public unsafe StorageSlice AllocateSlice()
        {
            var slicePagePos = _pager.Create(SlicePageCount);
            if (slicePagePos == -1)
            {
                throw new IndexOutOfRangeException(nameof(slicePagePos));
            }

            var page = _pager.GetBuffer(slicePagePos);
            if (page == null)
            {
                throw new NullReferenceException(nameof(page));
            }

            //初始化新分配的slice页
            fixed (byte* buffer = page)
            {
                var header = (PageHeader*)buffer;
                var entry = (StorageSliceSpaceUsageEntry*)&buffer[Constants.PageHeaderSize];

                header->PagePos = slicePagePos;
                header->FreeSize = 0;
                header->PrePagePos = -1;
                header->NextPagePos = -1;
                header->ItemCount = SlicePageCount;
                header->PageSize = Constants.PageSize;
                header->LastUsedPos = Constants.PageSize - 1;
                header->ModifiedCount += Constants.PageSize;

                for (var i = 0; i < SlicePageCount; i++)
                {
                    entry[i].Pos = slicePagePos + i;
                    entry[i].UsedSize = i == 0 ? Constants.PageSize : Constants.PageHeaderSize;
                }
            }

            return new StorageSlice(new StorageSlicePage(page), _pager);
        }
    }
}
