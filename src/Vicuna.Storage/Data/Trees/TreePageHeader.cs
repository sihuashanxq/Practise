using System;
using System.Runtime.InteropServices;
using Vicuna.Storage.Pages;

namespace Vicuna.Storage.Data.Trees
{
    [Flags]
    public enum TreeNodeFlags : byte
    {
        None = 0,

        Leaf = 1,

        Branch = 2
    }

    public enum DataValueType : byte
    {
        None = 0,

        Int = 1,

        Bool = 2,

        Date = 3,

        Byte = 4,

        Long = 5,

        Short = 6,

        Float = 7,

        Double = 8,

        Object = 9,

        String = 10
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 96)]
    public unsafe struct TreePageHeader
    {
        [FieldOffset(0)]
        public PageFlags Flags;

        [FieldOffset(1)]
        public ushort Low;

        [FieldOffset(3)]
        public ushort High;

        [FieldOffset(5)]
        public ushort KeySize;

        [FieldOffset(7)]
        public long PageNumber;

        [FieldOffset(15)]
        public ushort ItemCount;

        [FieldOffset(17)]
        public TreeNodeFlags NodeFlags;

        [FieldOffset(18)]
        public fixed byte MetaKeys[32];

        [FieldOffset(50)]
        public fixed byte Reserved[46];
    }

    [StructLayout(LayoutKind.Explicit, Size = 2)]
    public struct MetadataKey
    {
        [FieldOffset(0)]
        public byte Size;

        [FieldOffset(1)]
        public DataValueType KeyType;
    }
}
