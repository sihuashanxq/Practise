using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Vicuna.Storage.Pages
{
    public unsafe class SegmentHeadPage : Page
    {
        private const int SegmentEntrySize = 2048;

        public bool HasFreeSpacePage { get; internal set; }

        public SortedSet<AllocationEntry> AllocationEntries { get; }

        public SegmentHeadPage(byte[] buffer) : base(buffer)
        {
            HasFreeSpacePage = true;
            AllocationEntries = new SortedSet<AllocationEntry>(AllocationEntryComparer.Comparer);
            BuildAllocationEntries();
        }

        protected void BuildAllocationEntries()
        {
            for (var entryIndex = 1; entryIndex < SegmentEntrySize; entryIndex++)
            {
                AllocationEntries.Add(new AllocationEntry((ushort)entryIndex, Buffer));
            }
        }

        [StructLayout(LayoutKind.Explicit, Size = 4, Pack = 1)]
        public struct AllocationEntry
        {
            [FieldOffset(0)]
            public ushort PageOffset;

            [FieldOffset(2)]
            public ushort MaxFreeSize;

            internal AllocationEntry(ushort pageOffset, byte[] buffer)
            {
                if (buffer == null)
                {
                    PageOffset = pageOffset;
                    MaxFreeSize = Constants.PageSize - Constants.PageHeaderSize;
                }
                else
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

        private class AllocationEntryComparer : IComparer<AllocationEntry>
        {
            public static readonly IComparer<AllocationEntry> Comparer = new AllocationEntryComparer();

            public int Compare(AllocationEntry x, AllocationEntry y)
            {
                var d = y.MaxFreeSize - x.MaxFreeSize;
                if (d == 0)
                {
                    return x.PageOffset - x.PageOffset;
                }

                return d;
            }
        }
    }
}
