using System;
using System.Runtime.InteropServices;
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
        Null = 1,

        Char = 2,

        Byte = 3,

        Array = 4,

        Int16 = 5,

        Int32 = 6,

        Int64 = 7,

        UInt16 = 8,

        UInt32 = 9,

        UInt64 = 10,

        Single = 11,

        Double = 12,

        Object = 13,

        String = 14,

        Boolean = 15,

        DateTime = 16
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 96)]
    public unsafe struct TreePageHeader
    {
        [FieldOffset(0)]
        public PageHeaderFlags Flags;

        [FieldOffset(1)]
        public int StoreId;

        [FieldOffset(5)]
        public long PageNumber;

        [FieldOffset(13)]
        public ushort Low;

        [FieldOffset(15)]
        public ushort Upper;

        [FieldOffset(17)]
        public ushort KeySize;

        [FieldOffset(19)]
        public ushort ItemCount;

        [FieldOffset(21)]
        public ushort UsedLength;

        [FieldOffset(23)]
        public ushort LastDeleted;

        [FieldOffset(25)]
        public TreeNodeFlags NodeFlags;

        [FieldOffset(26)]
        public fixed byte Reserved[70];
    }

    [StructLayout(LayoutKind.Explicit, Size = 2)]
    public struct MetadataKey
    {
        [FieldOffset(0)]
        public byte Size;

        [FieldOffset(1)]
        public DataValueType Type;
    }
}
