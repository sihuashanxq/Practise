using System;
using Vicuna.Storage.Pages;

namespace Vicuna.Storage
{

    public class StorageSegmentHandling
    {
        private StorageSliceHandling _sliceHandling;

        public StorageSegmentHandling(StorageSliceHandling sliceHandling)
        {
            _sliceHandling = sliceHandling;
        }

        public unsafe StorageSegment Allocate()
        {
            var slice = _sliceHandling.AllocateSlice();
            if (slice == null)
            {
                throw new NullReferenceException(nameof(slice));
            }

            if (!slice.AllocatePage(out var page))
            {
                return null;
            }

            fixed (byte* buffer = page.Buffer)
            {
                var header = (PageHeader*)buffer;
                var sliceEntry = (StorageSpaceUsageEntry*)&buffer[Constants.PageHeaderSize];

                header->ItemCount = 1;
                header->PagePos = page.PagePos;
                header->LastUsedPos = 0;
                header->PrePagePos = -1;
                header->NextPagePos = -1;
                header->FreeSize = 0;
                header->PageSize = Constants.PageSize;

                sliceEntry->Pos = slice.Usage.Pos;
                sliceEntry->UsedSize = slice.Usage.UsedSize;
            }

            return new StorageSegment(new StoragePage(page.Buffer), _sliceHandling);
        }
    }
}
