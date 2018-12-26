using System;
using System.Collections.Generic;
using Vicuna.Storage.Pages;

namespace Vicuna.Storage
{
    public unsafe class StorageSiceHeadPage : Page
    {
        internal const int SlicePageCount = 1024;

        public int Count => SlicePageCount;

        public StorageSiceHeadPage(byte[] buffer) : base(buffer)
        {

        }

        public void SetPageEntry(SlicePageUsageEntry entry)
        {
            fixed (byte* buffer = Buffer)
            {
                ((StorageSliceSpaceUsage*)&buffer[Constants.PageHeaderSize])[entry.Index] = entry.Usage;
            }
        }

        public SlicePageUsageEntry GetPageEntry(int index)
        {
            if (index < 0 || index > SlicePageCount)
            {
                throw new IndexOutOfRangeException(nameof(index));
            }

            fixed (byte* buffer = Buffer)
            {
                return GetPageEntry((StorageSliceSpaceUsage*)&buffer[Constants.PageHeaderSize], index);
            }
        }

        private SlicePageUsageEntry GetPageEntry(StorageSliceSpaceUsage* pointer, int index)
        {
            if (index < 0 || index > SlicePageCount)
            {
                throw new IndexOutOfRangeException(nameof(index));
            }

            var usage = pointer[index];
            if (usage.UsedLength == 0)
            {
                usage.UsedLength = (int)Constants.PageHeaderSize;
            }

            if (usage.PageOffset != PagePos + index)
            {
                usage.PageOffset = PagePos + index;
            }

            return new SlicePageUsageEntry(index, usage);
        }

        public List<SlicePageUsageEntry> GetPageEntries()
        {
            var pageUsagEntries = new List<SlicePageUsageEntry>();

            fixed (byte* buffer = Buffer)
            {
                var pointer = (StorageSliceSpaceUsage*)&buffer[Constants.PageHeaderSize];

                for (var index = 0; index < Count; index++)
                {
                    pageUsagEntries.Add(GetPageEntry(pointer, index));
                }
            }

            return pageUsagEntries;
        }
    }
}
