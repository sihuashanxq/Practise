using System.Runtime.InteropServices;

namespace Vicuna.Storage.Slices
{
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 14)]
    public struct StorageSliceUsage
    {
        [FieldOffset(0)]
        public long PageNumber;

        [FieldOffset(8)]
        public int UsedLength;

        [FieldOffset(12)]
        public short FreePageCount;

        public StorageSliceUsage(long pageNumber, int usedLength, short freePageCount)
        {
            PageNumber = pageNumber;
            UsedLength = usedLength;
            FreePageCount = freePageCount;
        }
    }
}
