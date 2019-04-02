using System;
using System.Runtime.InteropServices;
using Vicuna.Storage.Pages;
using Vicuna.Storage.Paging;

namespace Vicuna.Storage.Data.Trees
{
    [Flags]
    public enum TreeNodeFlags : byte
    {
        None = 0,

        Leaf = 1,

        Branch = 2,

        Primary = 4
    }

    /// <summary>
    /// low  6-bits is the value's type
    /// high 2-bits is the value size used byte count( string or object)
    /// such as "helloworld", the structure is a byte[] (0x0A0x0Ahelloworld)
    /// </summary>
    public enum DataValueType : byte
    {
        None = 255,

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
        public PageHeaderFlags Flags;

        [FieldOffset(1)]
        public ushort Low;

        [FieldOffset(3)]
        public ushort Upper;

        [FieldOffset(5)]
        public ushort KeySize;

        [FieldOffset(7)]
        public long PageNumber;

        [FieldOffset(15)]
        public ushort ItemCount;

        [FieldOffset(17)]
        public ushort UsedLength;

        [FieldOffset(19)]
        public ushort LastDeleted;

        [FieldOffset(21)]
        public TreeNodeFlags NodeFlags;

        [FieldOffset(22)]
        public fixed byte MetaKeys[16];

        [FieldOffset(54)]
        public fixed byte Reserved[42];
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
