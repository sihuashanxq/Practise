using System.Runtime.InteropServices;

namespace Vicuna.Storage.Slices
{
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 12)]
    public struct StorageSlicePageUsage
    {
        [FieldOffset(0)]
        public long PageNumber;

        [FieldOffset(8)]
        public int UsedLength;

        public StorageSlicePageUsage(long pageNumber, int usedLength)
        {
            PageNumber = pageNumber;
            UsedLength = usedLength;
        }
    }
}
