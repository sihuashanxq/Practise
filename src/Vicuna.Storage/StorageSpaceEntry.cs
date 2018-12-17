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

    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 10)]
    internal struct StorageSliceSpaceEntry
    {
        public const int SizeOf = 10;

        [FieldOffset(0)]
        public long Pos;

        [FieldOffset(8)]
        public short UsedSize;
    }
}
