using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Vicuna.Storage.Data.Trees
{
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
            size += TreeNodeHeader.SizeOf;                                                                  //+ header-size
            size += flags == TreeNodeHeaderFlags.Data ? TreeNodeTransactionHeader.SizeOf : (ushort)0;       //+ trans-header-size

            return AllocateInternal(index, size, flags, out position);
        }

        internal bool AllocateInternal(int index, ushort size, TreeNodeHeaderFlags flags, out ushort position)
        {
            if (Constants.PageSize - Header.UsedLength < size + sizeof(ushort))
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

                var from = Slice(moveStart, moveSize);
                var to = Slice(moveStart + sizeof(ushort), moveSize);

                from.CopyTo(to);
                Write(moveStart, (ushort)upper);
            }
            else
            {
                Write(Header.Low, (ushort)upper);
            }

            position = (ushort)upper;

            Header.Low += sizeof(ushort);
            Header.Upper = (ushort)upper;

            Header.UsedLength += (ushort)(size + sizeof(ushort));
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

            if (nodeKey.Size > 0)
            {
                nodeKey.CopyTo(key);
            }
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
            var start = (index + 1) * sizeof(ushort) + Constants.PageHeaderSize;
            var size = Header.Low - start;

            var to = Slice(indexPosition, size);
            var from = Slice(start, size);

            from.CopyTo(to);
            Write(Header.Low, (ushort)0);

            Header.Low -= sizeof(ushort);
            Header.ItemCount--;
            Header.UsedLength -= node.GetNodeSize();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ref TreeNodeHeader GetNodeHeader(ushort position)
        {
            return ref Read<TreeNodeHeader>(position, TreeNodeHeader.SizeOf);
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

        public int Search(TreeNodeKey key, out int index)
        {
            var count = IsLeaf ? Header.ItemCount : Header.ItemCount - 1;
            if (count <= 0)
            {
                index = 0;
                return 1;
            }

            try
            {
                //<=first
                var nFlag = CompareTo(MinKey, key);
                if (nFlag >= 0)
                {
                    index = IsBranch && nFlag == 0 ? 1 : 0;
                    return IsBranch ? 0 : nFlag;
                }
            }
            catch
            {

            }

            //>=last 
            try
            {
                var xFlag = CompareTo(MaxKey, key);
                if (xFlag <= 0)
                {
                    index = IsBranch ? count : count - 1;
                    return IsBranch ? 0 : xFlag;
                }
            }
            catch
            {

            }

            return BinarySearch(key, 0, count - 1, out index);
        }

        public int Search(TreeNodeKey key, out int index, out ushort position)
        {
            var flag = Search(key, out index);
            if (flag == 0)
            {
                position = GetNodeOffset(index);
            }
            else
            {
                position = 0;
            }

            return flag;
        }

        public int Search(TreeNodeKey key, out int index, out ushort position, out TreeNodeHeader? node)
        {
            var flag = Search(key, out index);
            if (flag == 0)
            {
                position = GetNodeOffset(index);
                node = GetNodeHeader(position);
            }
            else
            {
                position = 0;
                node = null;
            }

            return flag;
        }

        public int BinarySearch(TreeNodeKey key, int first, int last, out int index)
        {
            while (first < last)
            {
                var mid = first + (last - first) / 2;
                var midKey = GetNodeKey(mid);
                var flag = CompareTo(midKey, key);
                if (flag == 0)
                {
                    index = IsLeaf ? mid : mid + 1;
                    return flag;
                }

                if (flag > 0)
                {
                    last = mid;
                    continue;
                }

                first = mid + 1;
            }

            var lastKey = GetNodeKey(last);
            var lastFlag = CompareTo(lastKey, key);
            if (lastFlag == 0)
            {
                index = IsLeaf ? last : last + 1;
                return 0;
            }

            //awalys >,branch matched
            index = last;
            return IsBranch ? 0 : lastFlag;
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

        private short ValueSizeOf(DataValueType type, int start, Span<byte> values)
        {
            switch (type)
            {
                case DataValueType.Int:
                    return sizeof(int);
                case DataValueType.Bool:
                    return sizeof(bool);
                case DataValueType.Byte:
                    return sizeof(char);
                case DataValueType.Date:
                case DataValueType.Long:
                    return sizeof(long);
                case DataValueType.Short:
                    return sizeof(short);
                case DataValueType.Float:
                    return sizeof(float);
                case DataValueType.Double:
                    return sizeof(double);
                case DataValueType.String:
                    return values[start];
            }

            return 0;
        }

        private int CompareTo(TreeNodeKey keyX, TreeNodeKey keyY)
        {
            fixed (byte* p = Header.MetaKeys)
            {
                var m = (short)0;
                var n = (short)0;
                var ptr = (DataValueType*)p;

                while (*ptr != DataValueType.None)
                {
                    var valueType = *ptr;
                    var xSize = ValueSizeOf(valueType, m, keyX.Keys);
                    var ySize = ValueSizeOf(valueType, n, keyY.Keys);

                    if (valueType == DataValueType.String)
                    {
                        m++;
                        n++;
                    }

                    var flag = CompareTo(keyX, keyY, m, n, xSize, ySize, valueType);
                    if (flag != 0)
                    {
                        return flag;
                    }

                    m += xSize;
                    n += ySize;
                    ptr++;
                }

                return 0;
            }
        }

        private int CompareTo(TreeNodeKey keyX, TreeNodeKey keyY, short xIndex, short yIndex, short xSize, short ySize, DataValueType valueType)
        {
            switch (valueType)
            {
                case DataValueType.Int:
                    return CompareTo(Unsafe.As<byte, int>(ref keyX[xIndex]), Unsafe.As<byte, int>(ref keyY[yIndex]));
                case DataValueType.Date:
                case DataValueType.Long:
                    return CompareTo(Unsafe.As<byte, long>(ref keyX[xIndex]), Unsafe.As<byte, long>(ref keyY[yIndex]));
                case DataValueType.Short:
                    return CompareTo(Unsafe.As<byte, short>(ref keyX[xIndex]), Unsafe.As<byte, short>(ref keyY[yIndex]));
                case DataValueType.Float:
                    return CompareTo(Unsafe.As<byte, float>(ref keyX[xIndex]), Unsafe.As<byte, float>(ref keyY[yIndex]));
                case DataValueType.Double:
                    return CompareTo(Unsafe.As<byte, double>(ref keyX[xIndex]), Unsafe.As<byte, double>(ref keyY[yIndex]));
                case DataValueType.Byte:
                case DataValueType.Bool:
                case DataValueType.String:
                    return CompareTo(ref keyX[xIndex], ref keyY[yIndex], xSize, ySize);
                default:
                    throw new NotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int CompareTo(ref byte x, ref byte y, int xSize, int ySize)
        {
            if (xSize == 1 && ySize == 1)
            {
                return x - y;
            }

            fixed (byte* p1 = &x, p2 = &y)
            {
                return ByteStringComparer.CompareTo(p1, p2, xSize, ySize);
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
            var index = length;

            Span<byte> buffer = new byte[length];

            for (var i = 0; i < count; i++)
            {
                var nIndex = GetIndexOffset(i);
                var nOffset = GetNodeOffset(i);
                ref var node = ref GetNodeHeader(nOffset);
                var size = node.GetNodeSize() - sizeof(ushort);

                index -= size;

                var to = buffer.Slice(index, size);
                var from = Slice(nOffset, size);

                from.CopyTo(to);
                Write(nIndex, (ushort)(Header.Upper + index));
            }

            var moveTo = Slice(Header.Upper, length);
            var moveFrom = buffer;

            moveFrom.CopyTo(moveTo);

            Header.Upper += (ushort)index;

            if (Header.Upper > Constants.PageSize)
            {

            }
        }

        internal CopyEntriesResult CopyRightSideEntriesToNewPage(int index, TreePage newPage)
        {
            var size = 0;
            var newIndex = 0;
            var count = Header.ItemCount;

            for (var i = index + 1; i < count; i++)
            {
                CopyNodeEntryToNewPage(i, newIndex, newPage, out var nodeSize);

                newIndex++;
                size += nodeSize;
            }

            //let current page has more free space 
            if (newPage.Header.UsedLength < Header.UsedLength)
            {
                CopyNodeEntryToNewPage(index, 0, newPage, out var nodeSize);
                Header.Low -= (ushort)((count - index) * 2);
                Header.UsedLength -= (ushort)(size);
                Header.UsedLength -= nodeSize;
                if (Header.Upper > Constants.PageSize)
                {

                }

                if (newPage.Header.Upper > Constants.PageSize)
                {

                }

                Header.ItemCount -= (ushort)((count - index));
                return CopyEntriesResult.StartNodeMovedToNewPage;
            }

            Header.Low -= (ushort)((count - index - 1) * 2);
            Header.UsedLength -= (ushort)(size);
            Header.ItemCount -= (ushort)((count - index - 1));
            if (Header.Upper > Constants.PageSize)
            {

            }

            if (newPage.Header.Upper > Constants.PageSize)
            {

            }

            return CopyEntriesResult.Normal;
        }

        internal void CopyNodeEntryToNewPage(int sourceIndex, int newIndex, TreePage newPage, out ushort nodeSize)
        {
            var offset = GetNodeOffset(sourceIndex);
            var node = GetNodeHeader(offset);
            var size = node.GetNodeSize() - sizeof(ushort);

            if (!newPage.AllocateInternal(newIndex, (ushort)size, node.NodeFlags, out var newNodeOffset))
            {
                throw new Exception("tree page split failed!");
            }

            var oldNode = Slice(offset, size - sizeof(ushort));
            var newNode = newPage.Slice(newNodeOffset, size);

            oldNode.CopyTo(newNode);

            nodeSize = (ushort)size;
        }

        public List<byte[]> Keys
        {
            get
            {
                var keys = new List<byte[]>();

                for (var i = 0; i < Header.ItemCount; i++)
                {
                    var key = GetNodeKey(i);
                    if (key.Size > 0)
                    {
                        keys.Add(key.Keys.ToArray());
                    }
                }

                return keys;
            }
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
                    return (ushort)(SizeOf + nodePosition + KeySize + TreeNodeTransactionHeader.SizeOf);
                case TreeNodeHeaderFlags.DataRef:
                    return (ushort)(SizeOf + nodePosition + KeySize);
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

    public enum CopyEntriesResult
    {
        Normal = 0,

        StartNodeMovedToNewPage = 1,
    }
}