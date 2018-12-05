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

        public unsafe StorageSlice GetSlice(long slicePagePos)
        {
            var pageBuffer = _pager.GetPageBuffer(slicePagePos);
            if (pageBuffer == null)
            {
                throw new NullReferenceException(nameof(pageBuffer));
            }

            return new StorageSlice(new StorageSlicePage(pageBuffer), _pager);
        }

        public unsafe StorageSlice AllocateSlice()
        {
            var slicePagePos = _pager.Create(SlicePageCount);
            if (slicePagePos == -1)
            {
                throw new IndexOutOfRangeException(nameof(slicePagePos));
            }

            var pageBuffer = _pager.GetPageBuffer(slicePagePos);
            if (pageBuffer == null)
            {
                throw new NullReferenceException(nameof(pageBuffer));
            }

            //初始化新分配的slice页
            fixed (byte* buffer = pageBuffer)
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

                for (var i = 0; i < SlicePageCount; i++)
                {
                    entry[i].Pos = slicePagePos + i;
                    entry[i].UsedSize = i == 0 ? Constants.PageSize : Constants.PageHeaderSize;
                }
            }

            return new StorageSlice(new StorageSlicePage(pageBuffer), _pager);
        }
    }
}
