using System.Runtime.InteropServices;

namespace Vicuna.Storage
{
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 10)]
    public struct StorageSpaceEntry
    {
        [FieldOffset(0)]
        public long Pos;

        [FieldOffset(8)]
        public long UsedSize;

        public StorageSpaceEntry(long pos)
             : this(pos, 0)
        {

        }

        public StorageSpaceEntry(long pos, long usedSize)
        {
            Pos = pos;
            UsedSize = usedSize;
        }
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 12)]
    public struct StorageSliceSpaceUsage
    {
        public const int SizeOf = 12;

        public StorageSliceSpaceUsage(long pageOffset, int usedLength)
        {
            PageOffset = pageOffset;
            UsedLength = usedLength;
        }

        [FieldOffset(0)]
        public long PageOffset;

        [FieldOffset(8)]
        public int UsedLength;
    }

    public class SlicePageUsageEntry
    {
        public int Index { get; set; }

        public StorageSliceSpaceUsage Usage { get; set; }

        public SlicePageUsageEntry(int index, StorageSliceSpaceUsage usage)
        {
            Index = index;
            Usage = usage;
        }

        public SlicePageUsageEntry()
        {

        }
    }
}