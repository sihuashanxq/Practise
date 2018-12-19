using System;
using Vicuna.Storage.Pages;
using Vicuna.Storage.Transactions;

namespace Vicuna.Storage
{
    public class StorageSegmentManager
    {
        public StorageLevelTransaction _tx;

        private readonly StorageSliceManager _sliceManager;

        public StorageSegmentManager(StorageSliceManager sliceManager)
        {
            _sliceManager = sliceManager;
        }

        public unsafe StorageSegment Allocate()
        {
            var slice = _sliceManager.Allocate();
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
                var sliceEntry = (StorageSpaceEntry*)&buffer[Constants.PageHeaderSize];

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

            return new StorageSegment(new StoragePage(page.Buffer), _sliceManager);
        }
    }
}
