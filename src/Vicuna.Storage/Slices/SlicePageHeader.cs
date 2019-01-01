using System.Runtime.InteropServices;
using Vicuna.Storage.Pages;

namespace Vicuna.Storage.Slices
{
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 96)]
    public unsafe struct SlicePageHeader
    {
        [FieldOffset(0)]
        public PageHeaderFlag Flag;

        [FieldOffset(1)]
        public int CheckSum;

        [FieldOffset(5)]
        public short PageSize;

        [FieldOffset(7)]
        public short FreeSize;

        [FieldOffset(9)]
        public int UsedLength;

        [FieldOffset(13)]
        public short ItemCount;

        [FieldOffset(15)]
        public long PageOffset;

        [FieldOffset(23)]
        public long PrePageOffset;

        [FieldOffset(31)]
        public long NextPageOffset;

        [FieldOffset(39)]
        public short LastUsedOffset;

        [FieldOffset(41)]
        public long ModifiedCount;

        [FieldOffset(49)]
        public int ActivedNodeIndex;

        [FieldOffset(53)]
        public long ActivedNodeOffset;

        [FieldOffset(61)]
        public fixed byte Reserved[33];
    }
}
