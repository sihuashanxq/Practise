﻿using System.Collections.Generic;

namespace Vicuna.Storage
{
    public class StorageSlice
    {
        public long Loc { get; }

        private List<StorageSlicePageEntry> _usedPages;

        private List<StorageSlicePageEntry> _freePages;

        public StorageSlice(long loc)
        {
            Loc = loc;
        }

        public long AllocPage()
        {
            return -1;
        }
    }
}
