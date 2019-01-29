using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Vicuna.Storage.Data.Trees
{
    public unsafe class TreePage
    {
        public byte[] Data;

        public bool IsLeaf
        {
            get => (Header.NodeFlags & TreeNodeFlags.Leaf) == TreeNodeFlags.Leaf;
        }

        public bool IsBranch
        {
            get => (Header.NodeFlags & TreeNodeFlags.Branch) == TreeNodeFlags.Branch;
        }

        public ref ushort KeyOffsets
        {
            get => ref Unsafe.As<byte, ushort>(ref Data[Constants.PageHeaderSize]);
        }

        public ByteString MinKey
        {
            get => GetKey(0);
        }

        public ByteString MaxKey
        {
            get => GetKey(Header.ItemCount - (IsLeaf ? 1 : 2));
        }

        public ref TreePageHeader Header
        {
            get => ref Unsafe.As<byte, TreePageHeader>(ref Data[0]);
        }

        public void Insert(ByteString key, long value, int index)
        {
            if (index <= Header.ItemCount - 1)
            {
                ExpandLowRegion(index);
            }

            SetPageRef(key, value, index);
            Header.ItemCount++;
        }

        public void Remove(int index)
        {
            if (index < Header.ItemCount - 1)
            {
                CompactLowRegion(index);
            }

            Header.ItemCount--;
        }

        public void Remove(int index, out ByteString key, out long value)
        {
            (key, value) = GetPageRef(index);
            Remove(index);
        }

        public List<KeyValuePair<ByteString, long>> Remove(int index, int count)
        {
            if (index < 0)
            {
                throw new IndexOutOfRangeException($"index:{index}");
            }

            if (index + count > Header.ItemCount)
            {
                throw new IndexOutOfRangeException($"itemcount:{Header.ItemCount},index:{index},count:{count}");
            }

            var offset = GetOffset(index);
            var entries = new List<KeyValuePair<ByteString, long>>();

            for (var i = index; i < index + count; i++)
            {
                var (key, value) = GetPageRef(i);

                entries.Add(new KeyValuePair<ByteString, long>(key, value));
            }

            Header.ItemCount -= (ushort)count;
            Unsafe.InitBlockUnaligned(ref Unsafe.As<byte, byte>(ref Data[offset]), 0, (uint)(count * (Header.KeySize + Header.ValueSize)));
            return entries;
        }

        public bool Search(ByteString key, out int index)
        {
            var count = IsLeaf ? Header.ItemCount : Header.ItemCount - 1;
            if (count <= 0)
            {
                index = 0;
                return false;
            }

            //<=first
            var nValue = MinKey.CompareTo(key);
            if (nValue >= 0)
            {
                index = IsBranch && nValue == 0 ? 1 : 0;
                return IsBranch || nValue == 0;
            }

            //>=last
            var xValue = MaxKey.CompareTo(key);
            if (xValue <= 0)
            {
                index = IsBranch ? count : count - 1;
                return IsBranch || xValue == 0;
            }

            return BinarySearch(key, 0, count - 1, out index);
        }

        public bool Search(ByteString key, out int index, out long value)
        {
            if (Search(key, out index))
            {
                value = GetPageRefNumber(index);
                return true;
            }

            value = long.MinValue;
            return false;
        }

        public bool BinarySearch(ByteString key, int first, int last, out int index)
        {
            while (first < last)
            {
                var mid = first + (last - first) / 2;
                var midKey = GetKey(mid);
                var flag = midKey.CompareTo(key);
                if (flag == 0)
                {
                    index = IsLeaf ? mid : mid + 1;
                    return true;
                }

                if (flag > 0)
                {
                    last = mid;
                    continue;
                }

                first = mid + 1;
            }

            var lastKey = GetKey(last);
            if (lastKey == null)
            {
                throw new NullReferenceException($"page number:{Header.PageNumber},last index :{last},search key {key.ToString()}");
            }

            if (lastKey.CompareTo(key) == 0)
            {
                index = IsLeaf ? last : last + 1;
                return true;
            }

            //awalys >,branch matched
            index = last;
            return IsBranch;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ByteString GetKey(int index)
        {
            return GetKey(ref GetKeyRef(index));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ByteString GetKey(ref byte kRef)
        {
            return GetData(ref kRef, Header.KeySize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long GetPageRefNumber(int index)
        {
            return Unsafe.As<byte, long>(ref GetValueRef(index));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (ByteString, long) GetPageRef(int index)
        {
            var key = GetKey(index);
            var value = GetPageRefNumber(index);

            return (key, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPageRef(ByteString key, long pageNumber, int index)
        {
            ref var kRef = ref GetKeyRef(index);
            ref var vRef = ref Unsafe.Add(ref kRef, Header.KeySize);

            Unsafe.CopyBlockUnaligned(ref kRef, ref key.Ptr, Header.KeySize);
            Unsafe.CopyBlockUnaligned(ref vRef, ref Unsafe.As<long, byte>(ref pageNumber), sizeof(long));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPageRef(ByteString key, int index)
        {
            Unsafe.CopyBlockUnaligned(ref GetKeyRef(index), ref key.Ptr, Header.KeySize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPageRef(long pageNumber, int index)
        {
            Unsafe.CopyBlockUnaligned(ref GetValueRef(index), ref Unsafe.As<long, byte>(ref pageNumber), sizeof(long));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref byte GetKeyRef(int index)
        {
            return ref Data[GetOffset(index)];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref byte GetValueRef(int index)
        {
            return ref Data[GetOffset(index) + Header.KeySize];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ByteString GetData(ref byte ptr, int count)
        {
            var v = new ByteString(count);

            Unsafe.CopyBlockUnaligned(ref v.Ptr, ref ptr, (uint)count);

            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetOffset(int index)
        {
            if (index < 0)
            {
                throw new IndexOutOfRangeException(nameof(index));
            }

            var offset = Constants.PageHeaderSize + index * (Header.KeySize + Header.ValueSize);
            if (offset < Constants.PageHeaderSize || offset + Header.KeySize + Header.ValueSize > Constants.PageFooterOffset)
            {
                throw new IndexOutOfRangeException(nameof(index));
            }

            return offset;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ExpandLowRegion(int index)
        {
            //var offset = GetOffset(index);
            //var size = (Header.ItemCount - index) * (Header.KeySize + Header.ValueSize);
            //var buffer = new byte[size];

            //ref var dPtr = ref Unsafe.As<byte, byte>(ref Data[offset]);
            //ref var bPtr = ref Unsafe.As<byte, byte>(ref buffer[0]);

            //Unsafe.CopyBlockUnaligned(ref bPtr, ref dPtr, (ushort)size);
            //Unsafe.CopyBlockUnaligned(ref Unsafe.Add(ref dPtr, Header.KeySize + Header.ValueSize), ref bPtr, (ushort)size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CompactLowRegion(int index)
        {
            //var offset = GetOffset(index + 1);
            //var size = (Header.ItemCount - index) * (Header.KeySize + Header.ValueSize);
            //var buffer = new byte[size + Header.KeySize + Header.ValueSize];

            //ref var dPtr = ref Unsafe.As<byte, byte>(ref Data[offset]);
            //ref var bPtr = ref Unsafe.As<byte, byte>(ref buffer[0]);

            //Unsafe.CopyBlockUnaligned(ref bPtr, ref dPtr, (ushort)size);
            //Unsafe.CopyBlockUnaligned(ref Unsafe.Subtract(ref dPtr, Header.KeySize + Header.ValueSize), ref bPtr, (ushort)(size + Header.KeySize + Header.ValueSize));
        }

        private int Compare(ByteString x, ByteString y)
        {
            fixed (byte* p = Header.MetaKeys)
            {
                var index = (short)0;
                var mkPtr = (MetadataKey*)p;

                while (mkPtr->KeyType != DataValueType.None)
                {
                    if (x.Length < index + mkPtr->Size || y.Length < index + mkPtr->Size)
                    {
                        return x.Length - y.Length;
                    }

                    var value = Compare(x, y, index, mkPtr->Size, mkPtr->KeyType);
                    if (value != 0)
                    {
                        return value;
                    }

                    index += mkPtr->Size;
                }

                return 0;
            }
        }

        private int Compare(ByteString x, ByteString y, short index, short size, DataValueType valueType)
        {
            switch (valueType)
            {
                case DataValueType.Int:
                    return CompareInt32(ref x[index], ref y[index]);
                case DataValueType.Date:
                case DataValueType.Long:
                    return CompareInt64(ref x[index], ref y[index]);
                case DataValueType.Short:
                    return CompareInt16(ref x[index], ref y[index]);
                case DataValueType.Float:
                    return CompareSingle(ref x[index], ref y[index]);
                case DataValueType.Double:
                    return CompareDouble(ref x[index], ref y[index]);
                case DataValueType.Byte:
                case DataValueType.Bool:
                    return CompareByte(ref x[index], ref y[index], 1);
                case DataValueType.String:
                    return CompareByte(ref x[index], ref y[index], size);
                default:
                    throw new NotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int CompareByte(ref byte x, ref byte y, int count)
        {
            if (count == 1)
            {
                return x - y;
            }

            fixed (byte* p1 = &x, p2 = &y)
            {
                return ByteStringComparer.CompareTo(p1, p2, count, count);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int CompareInt32(ref byte x, ref byte y)
        {
            return Unsafe.As<byte, int>(ref x) - Unsafe.As<byte, int>(ref y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int CompareInt16(ref byte x, ref byte y)
        {
            return Unsafe.As<byte, int>(ref x) - Unsafe.As<byte, int>(ref y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int CompareInt64(ref byte x, ref byte y)
        {
            return (int)(Unsafe.As<byte, long>(ref x) - Unsafe.As<byte, long>(ref y));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int CompareSingle(ref byte x, ref byte y)
        {
            return Unsafe.As<byte, float>(ref x).CompareTo(Unsafe.As<byte, float>(ref y));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int CompareDouble(ref byte x, ref byte y)
        {
            return Unsafe.As<byte, double>(ref x).CompareTo(Unsafe.As<byte, double>(ref y));
        }
    }

    internal static class ByteRefExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ToInt64(this ref byte value)
        {
            return Unsafe.As<byte, long>(ref value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ToUInt64(this ref byte value)
        {
            return Unsafe.As<byte, ulong>(ref value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteString ToByteString(this ref byte value, uint count)
        {
            var byteString = new ByteString(count);

            Unsafe.CopyBlockUnaligned(ref byteString.Ptr, ref value, count);

            return byteString;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo(this ref byte source, ref byte destination, uint count)
        {
            Unsafe.CopyBlockUnaligned(ref destination, ref source, count);
        }
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct TreeNodeHeader
    {
        [FieldOffset(0)]
        public short KeySize;

        [FieldOffset(2)]
        public uint DataSize;

        [FieldOffset(2)]
        public long PageNumber;

        [FieldOffset(10)]
        public TreeNodeHeaderFlags NodeFlags;
    }

    public enum TreeNodeHeaderFlags
    {
        None = 0,

        Data = 1,

        SingleKey = 2,

        MultipleKey = 3
    }
}
