using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Vicuna.Storage.Data.Trees
{
    public class DataValueBuilder
    {
        private int _index;

        public ByteString Value { get; }

        public ref TreeNodeHeader NodeHeader => ref Value[0].To<TreeNodeHeader>();

        public DataValueBuilder(int size)
        {
            Value = new ByteString(size);
        }

        public void AddValue<T>(T value) where T : struct
        {
            var sizeOf = Unsafe.SizeOf<T>();
            if (sizeOf + _index > Value.Size)
            {
                throw new IndexOutOfRangeException();
            }

            Value[_index].Set(value);
            _index += sizeOf;
        }

        public void AddString(ByteString value)
        {
            if (value.Size + _index > Value.Size)
            {
                throw new IndexOutOfRangeException();
            }

            Value[_index].Set(value);
            _index += (int)value.Size;
        }


        public void Clear()
        {
            _index = 0;
            Unsafe.InitBlock(ref Value[0], 0, (uint)Value.Size);
        }
    }

    public unsafe class TreePage : AbstractPage
    {
        public TreePage() : base()
        {

        }

        public TreePage(byte[] data) : base(data)
        {

        }

        public bool IsLeaf
        {
            get => (Header.NodeFlags & TreeNodeFlags.Leaf) == TreeNodeFlags.Leaf;
        }

        public bool IsBranch
        {
            get => (Header.NodeFlags & TreeNodeFlags.Branch) == TreeNodeFlags.Branch;
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
            get => ref Unsafe.As<byte, TreePageHeader>(ref Ptr);
        }

        public bool InserNode(DataValueBuilder builder, int index)
        {
            switch (builder.NodeHeader.NodeFlags)
            {
                case TreeNodeHeaderFlags.Data:
                case TreeNodeHeaderFlags.KeyDataRef:
                    return InsertDataNodeInHighPosition(builder, index) || InsertDataNodeInFreePosition(builder, index);
                case TreeNodeHeaderFlags.KeyPageRef:
                    return InsertKeyNodeInHighPosition(builder, index) || InsertKeyNodeInFreePosition(builder, index);
                default:
                    throw new NotSupportedException(builder.NodeHeader.NodeFlags.ToString());
            }
        }

        private bool InsertKeyNodeInFreePosition(DataValueBuilder builder, int index)
        {
            if (Header.LastDeleted <= 0)
            {
                return false;
            }

            var position = Header.LastDeleted;
            var freeEntry = Read<FreeDataEntry>(position);
            if (freeEntry.Next > 0)
            {
                Header.LastDeleted = freeEntry.Next;
            }

            if (index <= Header.ItemCount - 1)
            {
                ExpandLowRegion(index);
            }

            //branch key's size must be fixed,unnecceary compare free entry's size>=
            WriteNodeEntry((ushort)(Constants.PageHeaderSize + index * sizeof(ushort)), position, builder);

            Header.Low += sizeof(ushort);
            Header.ItemCount++;
            Header.UsedLength += (ushort)(builder.Value.Size + sizeof(ushort));
            return true;
        }

        private bool InsertKeyNodeInHighPosition(DataValueBuilder builder, int index)
        {
            var low = (ushort)(Header.Low + sizeof(ushort));
            var position = (ushort)(Header.High - builder.Value.Size);
            if (position < low)
            {
                return false;
            }

            if (index <= Header.ItemCount - 1)
            {
                ExpandLowRegion(index);
            }

            //branch key's size must be fixed,unnecceary compare free entry's size>=
            WriteNodeEntry((ushort)(Constants.PageHeaderSize + index * sizeof(ushort)), position, builder);

            Header.Low = low;
            Header.High = position;
            Header.ItemCount++;
            Header.UsedLength += (ushort)(builder.Value.Size + sizeof(ushort));
            return true;
        }

        private bool InsertDataNodeInFreePosition(DataValueBuilder builder, int index)
        {
            if (Header.LastDeleted <= 0)
            {
                return false;
            }

            var padding = (ushort)0;
            var position = Header.LastDeleted;
            var freeEntry = Read<FreeDataEntry>(position, sizeof(FreeDataEntry));
            if (freeEntry.Size < builder.Value.Size)
            {
                return false;
            }

            if (builder.Value.Size < Constants.MinFreeSlotSize)
            {
                padding = (ushort)(Constants.MinFreeSlotSize - builder.Value.Size);
            }
            else
            {
                padding = (ushort)((ushort)(builder.Value.Size * Constants.DefaultPaddingRate) | Constants.MinPaddingSize);
            }

            var freeReduce = freeEntry.Size - builder.Value.Size - padding;
            if (freeReduce < Constants.MinFreeSlotSize)
            {
                padding = (ushort)(freeEntry.Size - builder.Value.Size);
                freeReduce = 0;
            }

            if (freeReduce != 0)
            {
                //set new free entry
                freeEntry.Size = (ushort)freeReduce;
                Header.LastDeleted = (ushort)(Header.LastDeleted + padding + builder.Value.Size);

                Write(Header.LastDeleted, freeEntry, sizeof(FreeDataEntry));
            }
            else if (freeEntry.Next > 0)
            {
                Header.LastDeleted = freeEntry.Next;
            }

            if (index <= Header.ItemCount - 1)
            {
                ExpandLowRegion(index);
            }

            if (padding > 0)
            {
                //set padding-size
                builder.NodeHeader.PaddingSize = padding;
            }

            WriteNodeEntry((ushort)(Constants.PageHeaderSize + index * sizeof(ushort)), position, builder);

            Header.Low += sizeof(ushort);
            Header.ItemCount++;
            Header.UsedLength += (ushort)(builder.Value.Size + padding + sizeof(ushort));
            return true;
        }

        private bool InsertDataNodeInHighPosition(DataValueBuilder builder, int index)
        {
            var low = (ushort)(Header.Low + sizeof(ushort));
            var padding = (ushort)((ushort)(builder.Value.Size * Constants.DefaultPaddingRate) | Constants.MinPaddingSize);
            var position = (ushort)(Header.High - builder.Value.Size);
            var reduce = position - low - padding;
            if (reduce < 0)
            {
                return false;
            }

            if (reduce < Constants.MinFreeSlotSize)
            {
                padding = (ushort)(position - low);
                position = low;
            }
            else
            {
                position -= padding;
            }

            if (index <= Header.ItemCount - 1)
            {
                ExpandLowRegion(index);
            }

            if (padding > 0)
            {
                //set padding-size
                builder.NodeHeader.PaddingSize = padding;
            }

            WriteNodeEntry((ushort)(Constants.PageHeaderSize + index * sizeof(ushort)), position, builder);

            Header.Low = low;
            Header.High = position;
            Header.ItemCount++;
            Header.UsedLength += (ushort)(builder.Value.Size + padding + sizeof(ushort));

            return true;
        }

        public void WriteNodeEntry(int keyOffset, ushort nodeOffset, DataValueBuilder builder)
        {
            Write(keyOffset, nodeOffset, sizeof(ushort));
            Write(nodeOffset, ref builder.Value.Ptr, builder.Value.Size);
        }

        public void RemoveNode(int index)
        {
            if (index > Header.ItemCount - 1)
            {
                return;
            }

            var nodeIndex = index * sizeof(ushort) + Constants.PageHeaderSize;
            if (nodeIndex >= Constants.PageSize)
            {
                throw new IndexOutOfRangeException(nameof(nodeIndex));
            }

            var treeNode = Data.Get<TreeNodeHeader>(nodeIndex);
            var nodeSize = (ushort)(treeNode.KeySize + treeNode.DataSize + treeNode.PaddSize);
            var freeEntry = new FreeDataEntry(Header.LastDeleted, nodeSize);

            Data.Set(index, freeEntry);
            CompactLowRegion(index);

            Header.ItemCount--;
            Header.UsedLength -= nodeSize;
        }

        public ByteString[] RemoveNode(int index, int count)
        {
            if (index < 0)
            {
                throw new IndexOutOfRangeException($"index:{index}");
            }

            if (index + count >= Header.ItemCount)
            {
                throw new IndexOutOfRangeException($"item-count:{Header.ItemCount},index:{index},count:{count}");
            }

            var entries = new ByteString[count];

            for (var i = index + count - 1; i >= index; i--)
            {
                var nodeIndex = Constants.PageHeaderSize + sizeof(ushort) * index;
                var nodeHeader = Data.Get<TreeNodeHeader>(nodeIndex);
                var nodeData = Data.Get<TreeNodeHeader>(nodeIndex);
            }

            Header.ItemCount -= (ushort)count;
            Unsafe.InitBlockUnaligned(ref Unsafe.As<byte, byte>(ref Data[offset]), 0, (uint)(count * (Header.KeySize + Header.ValueSize)));
            return entries;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ByteString GetNodeData(int index)
        {
            if (index > Header.ItemCount - 1)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            throw null;
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

            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ExpandLowRegion(int index)
        {
            var start = index * sizeof(ushort) + Constants.PageHeaderSize;
            var size = Header.Low - start;
            var buffer = new byte[size];

            ref var sPtr = ref Unsafe.As<byte, byte>(ref Data[start]);
            ref var bPtr = ref Unsafe.As<byte, byte>(ref buffer[0]);

            Unsafe.CopyBlockUnaligned(ref bPtr, ref sPtr, (ushort)size);
            Unsafe.CopyBlockUnaligned(ref Unsafe.Add(ref sPtr, sizeof(ushort)), ref bPtr, (ushort)size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CompactLowRegion(int index)
        {
            var start = (index + 1) * sizeof(ushort) + Constants.PageHeaderSize;
            var size = Header.Low - start;
            var buffer = new byte[size + sizeof(ushort)];

            ref var sPtr = ref Unsafe.As<byte, byte>(ref Data[start]);
            ref var bPtr = ref Unsafe.As<byte, byte>(ref buffer[0]);

            Unsafe.CopyBlockUnaligned(ref bPtr, ref sPtr, (ushort)size);
            Unsafe.CopyBlockUnaligned(ref Unsafe.Subtract(ref sPtr, sizeof(ushort)), ref bPtr, (ushort)buffer.Length);
        }

        private int Compare(ByteString x, ByteString y)
        {
            fixed (byte* p = Header.MetaKeys)
            {
                var index = (short)0;
                var mkPtr = (MetadataKey*)p;

                while (mkPtr->KeyType != DataValueType.None)
                {
                    if (x.Size < index + mkPtr->Size || y.Size < index + mkPtr->Size)
                    {
                        return (int)(x.Size - y.Size);
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

    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 11)]
    public struct TreeNodeHeader
    {
        public const int SizeOf = 11;

        [FieldOffset(0)]
        public ushort KeySize;

        [FieldOffset(2)]
        public uint DataSize;

        [FieldOffset(2)]
        public long PageNumber;

        [FieldOffset(6)]
        public ushort PaddingSize;

        [FieldOffset(10)]
        public TreeNodeHeaderFlags NodeFlags;
    }

    public enum TreeNodeHeaderFlags
    {
        None = 0,

        Data = 1,

        KeyPageRef = 2,

        KeyDataRef = 3
    }
}
