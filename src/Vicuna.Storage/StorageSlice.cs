using System.Collections.Generic;

namespace Vicuna.Storage
{
    public class StorageSlice
    {
        public long SliceLoc { get; }

        private List<StorageSlicePageEntry> _usedPages;

        private List<StorageSlicePageEntry> _freePages;

        public StorageSlice(long sliceLoc)
        {
            SliceLoc = sliceLoc;
        }

        public long AllocPage()
        {
            return -1;
        }
    }
}
