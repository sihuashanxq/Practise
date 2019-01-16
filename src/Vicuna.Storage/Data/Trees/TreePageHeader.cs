using System.Runtime.InteropServices;
using Vicuna.Storage.Pages;

namespace Vicuna.Storage.Data.Trees
{
    public enum TreeNodeType
    {
        Root,

        Branch,

        Leaf
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public unsafe struct TreePageHeader
    {
        [FieldOffset(FlagOffset)]
        public PageHeaderFlag Flag;

        [FieldOffset(NodeTypeOffset)]
        public TreeNodeType NodeType;

        [FieldOffset(CheckSumOffset)]
        public int CheckSum;

        [FieldOffset(PageSizeOffset)]
        public short PageSize;

        [FieldOffset(ItemCountOffset)]
        public short ItemCount;

        [FieldOffset(PageNumberOffset)]
        public long PageNumber;

        [FieldOffset(PrePageNumberOffset)]
        public long PrePageNumber;

        [FieldOffset(NextPageNumberOffset)]
        public long NextPageNumber;

        [FieldOffset(KeySizeOffset)]
        public ushort KeySize;

        [FieldOffset(ValueSizeOffset)]
        public ushort ValueSize;

        [FieldOffset(ReservedOffset)]
        public fixed byte Reserved[ReservedLength];

        public const int FlagOffset = 0;

        public const int NodeTypeOffset = 1;

        public const int CheckSumOffset = 2;

        public const int PageSizeOffset = 6;

        public const int ItemCountOffset = 8;

        public const int PageNumberOffset = 10;

        public const int PrePageNumberOffset = 18;

        public const int NextPageNumberOffset = 26;

        public const int KeySizeOffset = 34;

        public const int ValueSizeOffset = 36;

        public const int ReservedOffset = 38;

        public const int ReservedLength = 56;
    }
}
