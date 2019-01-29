using System.Runtime.InteropServices;
using Vicuna.Storage.Pages;

namespace Vicuna.Storage.Slices
{
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 96)]
    public unsafe struct SlicePageHeader
    {
        [FieldOffset(0)]
        public PageFlags Flag;

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
        public long PageNumber;

        [FieldOffset(23)]
        public long PrePageNumber;

        [FieldOffset(31)]
        public long NextPageNumber;

        [FieldOffset(39)]
        public short LastUsedIndex;

        [FieldOffset(41)]
        public long ModifiedCount;

        [FieldOffset(49)]
        public int AcitvedNodeIndex;

        [FieldOffset(53)]
        public long AcitvedNodePageNumber;

        [FieldOffset(61)]
        public fixed byte Reserved[33];
    }
}
