using System.Runtime.InteropServices;

namespace Vicuna.Storage.Pages
{
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 96)]
    public unsafe struct PageHeader
    {
        public static PageHeader Default = new PageHeader() { PageNumber = -1 };

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
        public short FreeEntryIndex;

        [FieldOffset(51)]
        public short FreeEntryLength;

        [FieldOffset(53)]
        public fixed byte Reserved[41];
    }
}
