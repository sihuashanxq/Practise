using System;
using System.Runtime.InteropServices;

namespace Vicuna.Storage.Pages
{
    public unsafe class SegmentPage : Page
    {
        private const int SegmentEntrySize = 2048;

        public bool HasFreeSpacePage { get; internal set; }

        public AllocationEntry[] AllocationEntries { get; }

        public SegmentPage(byte[] buffer) : base(buffer)
        {
            HasFreeSpacePage = true;
            AllocationEntries = new AllocationEntry[SegmentEntrySize];
            BuildAllocationEntries();
        }

        protected void BuildAllocationEntries()
        {
            for (var entryIndex = 0; entryIndex < SegmentEntrySize; entryIndex++)
            {
                if (entryIndex == 0)
                {
                    AllocationEntries[entryIndex] = new AllocationEntry((ushort)entryIndex, 0);
                    continue;
                }

                if (Buffer != null)
                {
                    AllocationEntries[entryIndex] = new AllocationEntry((ushort)entryIndex, Buffer);
                    continue;
                }

                AllocationEntries[entryIndex] = new AllocationEntry((ushort)entryIndex, Constants.PageSize - Constants.PageHeaderSize);
            }
        }

        [StructLayout(LayoutKind.Explicit, Size = 4, Pack = 1)]
        public struct AllocationEntry
        {
            [FieldOffset(0)]
            public ushort PageOffset;

            [FieldOffset(2)]
            public ushort MaxFreeSize;

            public AllocationEntry(ushort pageOffset, ushort maxFreeSize)
            {
                PageOffset = pageOffset;
                MaxFreeSize = maxFreeSize;
            }

            internal AllocationEntry(ushort pageOffset, byte[] buffer)
            {
                var offset = pageOffset * sizeof(AllocationEntry);
                if (offset > buffer.Length - sizeof(AllocationEntry))
                {
                    throw new IndexOutOfRangeException();
                }

                PageOffset = pageOffset;
                MaxFreeSize = BitConverter.ToUInt16(buffer, offset + sizeof(ushort) + sizeof(byte));
            }
        }
    }
}
