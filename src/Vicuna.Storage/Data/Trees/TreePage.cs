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
        public const ushort MaxPerPageNodeDataSize = 8000;

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

        public TreeNodeKey MinKey
        {
            get => GetNodeKey(0);
        }

        public TreeNodeKey MaxKey
        {
            get => GetNodeKey(Header.ItemCount - (IsLeaf ? 1 : 2));
        }

        public ref TreePageHeader Header
        {
            get => ref Unsafe.As<byte, TreePageHeader>(ref Ptr);
        }

        public bool Allocate(int index, ushort size, TreeNodeHeaderFlags flags, out ushort position)
        {
            size += sizeof(ushort);                                                                         //+ index
            size += TreeNodeHeader.SizeOf;                                                                  //+ header-size
            size += flags == TreeNodeHeaderFlags.Data ? TreeNodeTransactionHeader.SizeOf : (ushort)0;       //+ trans-header-size

            return AllocateInternal(index, size, flags, out position);
        }

        internal bool AllocateInternal(int index, ushort size, TreeNodeHeaderFlags flags, out ushort position)
        {
            if (Constants.PageSize - Header.UsedLength < size)
            {
                position = 0;
                return false;
            }

            var low = Header.Low + sizeof(ushort);
            var upper = Header.Upper - size;
            if (upper < low)
            {
                CompactPage();
                upper = (ushort)(Header.Upper - size);
            }

            if (index <= Header.ItemCount - 1)
            {
                //move index region
                var moveStart = index * sizeof(ushort) + Constants.PageHeaderSize;
                var moveSize = Header.Low - moveStart;

                var to = Slice(moveStart, moveSize);
                var from = Slice(moveStart + sizeof(ushort), moveSize);

                from.CopyTo(to);
                Write(moveStart, (ushort)upper);

                Header.Low += sizeof(ushort);
            }
            else
            {
                Write(Header.Low, (ushort)upper);
            }

            position = (ushort)upper;

            Header.Upper = (ushort)upper;
            Header.UsedLength += size;
            Header.ItemCount++;

            return true;
        }

        public void InsertDataNode(int index, ushort offset, TreeNodeKey nodeKey, TreeNodeValue nodeValue, long txNumber)
        {
            ref var node = ref GetNodeHeader(offset);
            ref var tx = ref GetNodeTransactionHeader((ushort)(offset + nodeKey.Keys.Length + TreeNodeHeader.SizeOf));

            var key = Slice(offset + TreeNodeHeader.SizeOf, nodeKey.Keys.Length);
            var value = Slice(offset + TreeNodeHeader.SizeOf + TreeNodeTransactionHeader.SizeOf + nodeKey.Keys.Length, (int)nodeValue.Size);

            node.KeySize = nodeKey.Size;
            node.DataSize = nodeValue.Size;
            node.IsDeleted = false;
            node.NodeFlags = TreeNodeHeaderFlags.Data;

            tx.TransactionNumber = txNumber;
            tx.TransactionLogNumber = -1;

            nodeKey.CopyTo(key);
            nodeValue.CopyTo(value);
        }

        public void InsertDataRefNode(int index, ushort offset, TreeNodeKey nodeKey, TreeNodeValue nodeValue)
        {
            ref var node = ref GetNodeHeader(offset);

            var key = Slice(offset + TreeNodeHeader.SizeOf, nodeKey.Keys.Length);
            var value = Slice(offset + TreeNodeHeader.SizeOf + nodeKey.Keys.Length, (int)nodeValue.Size);

            node.KeySize = nodeKey.Size;
            node.DataSize = nodeValue.Size;
            node.IsDeleted = false;
            node.NodeFlags = TreeNodeHeaderFlags.Data;

            nodeKey.CopyTo(key);
            nodeValue.CopyTo(value);
        }

        public void InsertPageRefNode(int index, ushort offset, TreeNodeKey nodeKey, long pageNumber)
        {
            ref var node = ref GetNodeHeader(offset);
            var key = Slice(offset + TreeNodeHeader.SizeOf, nodeKey.Keys.Length);

            node.KeySize = nodeKey.Size;
            node.PageNumber = pageNumber;
            node.IsDeleted = false;
            node.NodeFlags = TreeNodeHeaderFlags.PageRef;

            nodeKey.CopyTo(key);
        }

        public void RemoveNode(int index, long txNumber, long txLogNumber)
        {
            var nodePosition = GetNodeOffset(index);
            var indexPosition = GetIndexOffset(index);

            // move index region
            var moveStart = (index + 1) * sizeof(ushort) + Constants.PageHeaderSize;
            var moveSize = Header.Low - moveStart;

            var to = Slice(indexPosition, moveSize);
            var from = Slice(moveStart, moveSize);

            from.CopyTo(to);
            Write(Header.Low, (ushort)0);

            //tx info
            ref var node = ref GetNodeHeader(nodePosition);
            if (node.NodeFlags == TreeNodeHeaderFlags.Data)
            {
                ref var tx = ref GetNodeTransactionHeader((ushort)(nodePosition + node.KeySize + TreeNodeTransactionHeader.SizeOf));

                tx.TransactionNumber = txNumber;
                tx.TransactionLogNumber = txLogNumber;
            }

            node.IsDeleted = true;
        }

        public void RemoveNode(int index)
        {
            var nodePosition = GetNodeOffset(index);
            var indexPosition = GetIndexOffset(index);
            ref var node = ref GetNodeHeader(nodePosition);

            // move index region
            var moveStart = (index + 1) * sizeof(ushort) + Constants.PageHeaderSize;
            var moveSize = Header.Low - moveStart;

            var to = Slice(indexPosition, moveSize);
            var from = Slice(moveStart, moveSize);

            from.CopyTo(to);
            Write(Header.Low, (ushort)0);

            Header.Low -= sizeof(ushort);
            Header.ItemCount--;
            Header.UsedLength -= node.GetNodeSize();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ref TreeNodeHeader GetNodeHeader(ushort nodePos)
        {
            return ref Read<TreeNodeHeader>(nodePos, TreeNodeHeader.SizeOf);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ref TreeNodeTransactionHeader GetNodeTransactionHeader(ushort txPos)
        {
            return ref Read<TreeNodeTransactionHeader>(txPos, TreeNodeTransactionHeader.SizeOf);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ushort GetNodeOffset(int index)
        {
            return Read<ushort>(GetIndexOffset(index));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ushort GetIndexOffset(int index)
        {
            var offset = (ushort)(index * sizeof(ushort) + Constants.PageHeaderSize);
            if (offset >= Constants.PageSize - sizeof(ushort))
            {
                throw new ArgumentOutOfRangeException($"PageNumber:{Header.PageNumber},Index:{index} offset out of page range!");
            }

            return offset;
        }

        public bool Search(TreeNodeKey key, out int index)
        {
            var count = IsLeaf ? Header.ItemCount : Header.ItemCount - 1;
            if (count <= 0)
            {
                index = 0;
                return false;
            }

            //<=first
            var nFlag = CompareTo(MinKey, key);
            if (nFlag >= 0)
            {
                index = IsBranch && nFlag == 0 ? 1 : 0;
                return IsBranch || nFlag == 0;
            }

            //>=last 
            var xFlag = CompareTo(MaxKey, key);
            if (xFlag <= 0)
            {
                index = IsBranch ? count : count - 1;
                return IsBranch || xFlag == 0;
            }

            return BinarySearch(key, 0, count - 1, out index);
        }

        public bool Search(TreeNodeKey key, out int index, out ushort position)
        {
            if (Search(key, out index))
            {
                position = GetNodeOffset(index);
                return true;
            }

            position = 0;
            return false;
        }

        public bool Search(TreeNodeKey key, out int index, out ushort position, out TreeNodeHeader? node)
        {
            if (Search(key, out index))
            {
                position = GetNodeOffset(index);
                node = GetNodeHeader(position);
                return true;
            }

            position = 0;
            node = null;
            return false;
        }

        public bool BinarySearch(TreeNodeKey key, int first, int last, out int index)
        {
            while (first < last)
            {
                var mid = first + (last - first) / 2;
                var midKey = GetNodeKey(mid);
                var flag = CompareTo(midKey, key);
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

            var lastKey = GetNodeKey(last);

            if (CompareTo(lastKey, key) == 0)
            {
                index = IsLeaf ? last : last + 1;
                return true;
            }

            //awalys >,branch matched
            index = last;
            return IsBranch;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TreeNodeKey GetNodeKey(int index)
        {
            var offset = GetNodeOffset(index);
            ref var node = ref GetNodeHeader(offset);

            return GetNodeKey(offset, ref node);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TreeNodeKey GetNodeKey(ushort nodeOffset, ref TreeNodeHeader node)
        {
            var key = Slice(nodeOffset + TreeNodeHeader.SizeOf, node.KeySize);

            return new TreeNodeKey(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TreeNodeValue GetNodeValue(int index)
        {
            var offset = GetNodeOffset(index);
            ref var node = ref GetNodeHeader(offset);

            return GetNodeValue(offset, ref node);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TreeNodeValue GetNodeValue(ushort nodeOffset, ref TreeNodeHeader node)
        {
            var offset = node.GetNodeDataOffset(nodeOffset);
            var value = Slice(offset, (int)node.DataSize);

            return new TreeNodeValue(value);
        }

        private int CompareTo(TreeNodeKey keyX, TreeNodeKey keyY)
        {
            fixed (byte* p = Header.MetaKeys)
            {
                var index = (short)0;
                var valueType = (DataValueType*)p;

                while (*valueType != DataValueType.None)
                {
                    if (keyX.Size < index + valueType->Size || keyY.Size < index + valueType->Size)
                    {
                        return keyX.Size - keyY.Size;
                    }

                    var size = 0;

                    if (*valueType == DataValueType.String)
                    {

                    }

                    var value = Compare(keyX, keyY, index, valueType->Size, valueType->KeyType);
                    if (value != 0)
                    {
                        return value;
                    }

                    index += valueType->Size;
                }

                return 0;
            }
        }

        private int CompareTo(TreeNodeKey keyX, TreeNodeKey keyY, short index, short size, DataValueType valueType)
        {
            switch (valueType)
            {
                case DataValueType.Int:
                    return CompareTo(Unsafe.As<byte, int>(ref keyX[index]), Unsafe.As<byte, int>(ref keyY[index]));
                case DataValueType.Date:
                case DataValueType.Long:
                    return CompareTo(Unsafe.As<byte, long>(ref keyX[index]), Unsafe.As<byte, long>(ref keyY[index]));
                case DataValueType.Short:
                    return CompareTo(Unsafe.As<byte, short>(ref keyX[index]), Unsafe.As<byte, short>(ref keyY[index]));
                case DataValueType.Float:
                    return CompareTo(Unsafe.As<byte, float>(ref keyX[index]), Unsafe.As<byte, float>(ref keyY[index]));
                case DataValueType.Double:
                    return CompareTo(Unsafe.As<byte, double>(ref keyX[index]), Unsafe.As<byte, double>(ref keyY[index]));
                case DataValueType.Byte:
                case DataValueType.Bool:
                case DataValueType.String:
                    return CompareTo(ref keyX[index], ref keyY[index], size);
                default:
                    throw new NotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int CompareTo(ref byte x, ref byte y, int count)
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
        private int CompareTo(int x, int y)
        {
            return x - y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int CompareTo(short x, short y)
        {
            return x - y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int CompareTo(long x, long y)
        {
            return x.CompareTo(y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int CompareTo(float x, float y)
        {
            return x.CompareTo(y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int CompareTo(double x, double y)
        {
            return x.CompareTo(y);
        }

        private void CompactPage()
        {
            var count = Header.ItemCount;
            var length = Constants.PageSize - Header.Upper;
            var index = length - 1;

            Span<byte> buffer = new byte[length];

            for (var i = 0; i < count; i++)
            {
                ref var nodePos = ref Read<ushort>(sizeof(ushort) * i + Constants.PageHeaderSize);
                ref var node = ref GetNodeHeader(nodePos);
                var size = node.GetNodeSize();

                index -= size;

                var to = buffer.Slice(index, size);
                var from = Slice(nodePos, size);

                from.CopyTo(to);
            }

            var moveTo = Slice(Header.Upper, length);
            var moveFrom = buffer;

            moveFrom.CopyTo(moveTo);

            Header.Upper += (ushort)index;
        }

        internal void CopyRightSideEntriesToNewPage(int index, TreePage newPage, out bool isStartNodeMovedToNewPage)
        {
            var movedSize = 0;
            var movedIndex = 0;

            for (var i = index + 1; i < Header.ItemCount; i++)
            {
                CopyNodeEntryToNewPage(i, movedIndex, newPage, out var nodeSize);

                movedIndex++;
                movedSize += nodeSize;
            }

            //let current page has more free space 
            if (newPage.Header.UsedLength < Header.UsedLength)
            {
                CopyNodeEntryToNewPage(index, 0, newPage, out var _);
                isStartNodeMovedToNewPage = true;
            }
            else
            {
                isStartNodeMovedToNewPage = false;
            }
        }

        internal void CopyNodeEntryToNewPage(int sourceIndex, int newIndex, TreePage newPage, out ushort nodeSize)
        {
            var offset = GetNodeOffset(sourceIndex);
            var node = GetNodeHeader(offset);
            var size = (ushort)(node.GetNodeSize() + sizeof(ushort));

            if (!newPage.AllocateInternal(newIndex, size, node.NodeFlags, out var newNodeOffset))
            {
                throw new Exception("tree page split failed!");
            }

            var oldNode = Slice(offset, size - sizeof(ushort));
            var newNode = newPage.Slice(newNodeOffset, size);

            oldNode.CopyTo(newNode);

            nodeSize = size;
        }
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = SizeOf)]
    public struct TreeNodeHeader
    {
        public const ushort SizeOf = 12;

        public const ushort NodeSlotSize = sizeof(ushort);

        [FieldOffset(0)]
        public bool IsDeleted;

        [FieldOffset(1)]
        public ushort KeySize;

        [FieldOffset(3)]
        public uint DataSize;

        [FieldOffset(3)]
        public long PageNumber;

        [FieldOffset(11)]
        public TreeNodeHeaderFlags NodeFlags;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort GetNodeSize()
        {
            switch (NodeFlags)
            {
                case TreeNodeHeaderFlags.Data:
                    return (ushort)(SizeOf + NodeSlotSize + KeySize + DataSize + TreeNodeTransactionHeader.SizeOf);
                case TreeNodeHeaderFlags.DataRef:
                    return (ushort)(SizeOf + NodeSlotSize + KeySize + DataSize);
                default:
                    return (ushort)(SizeOf + NodeSlotSize + KeySize);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort GetNodeDataOffset(ushort nodePosition)
        {
            switch (NodeFlags)
            {
                case TreeNodeHeaderFlags.Data:
                    return (ushort)(SizeOf + nodePosition + KeySize + DataSize + TreeNodeTransactionHeader.SizeOf);
                case TreeNodeHeaderFlags.DataRef:
                    return (ushort)(SizeOf + nodePosition + KeySize + DataSize);
                default:
                    return (ushort)(SizeOf + nodePosition + KeySize);
            }
        }
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = SizeOf)]
    public struct TreeNodeTransactionHeader
    {
        public const ushort SizeOf = 16;

        [FieldOffset(0)]
        public long TransactionNumber;

        [FieldOffset(8)]
        public long TransactionLogNumber;
    }

    public enum TreeNodeHeaderFlags : byte
    {
        Data = 1,

        DataRef = 2,

        PageRef = 3
    }

    public ref struct TreeNodeKey
    {
        public ushort Size;

        public Span<byte> Keys;

        public ref byte this[int index] => ref Keys[index];

        public TreeNodeKey(Span<byte> keys)
        {
            Keys = keys;
            Size = (ushort)keys.Length;
        }

        public void CopyTo(Span<byte> dest)
        {
            Keys.CopyTo(dest);
        }
    }

    public ref struct TreeNodeValue
    {
        public uint Size;

        public Span<byte> Values;

        public TreeNodeValue(Span<byte> values)
        {
            Values = values;
            Size = (uint)values.Length;
        }

        public void CopyTo(Span<byte> dest)
        {
            Values.CopyTo(dest);
        }
    }
}
