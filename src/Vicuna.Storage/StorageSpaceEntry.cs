using System.Runtime.InteropServices;

namespace Vicuna.Storage
{
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 12)]
    public struct SpaceUsage
    {
        [FieldOffset(0)]
        public long PageOffset;

        [FieldOffset(8)]
        public int UsedLength;

        public SpaceUsage(long pageOffset, int usedLength)
        {
            PageOffset = pageOffset;
            UsedLength = usedLength;
        }
    }
    
    public class SlicePageUsageEntry
    {
        public int Index { get; set; }

        public SpaceUsage Usage { get; set; }

        public SlicePageUsageEntry(int index, SpaceUsage usage)
        {
            Index = index;
            Usage = usage;
        }

        public SlicePageUsageEntry()
        {

        }
    }
}