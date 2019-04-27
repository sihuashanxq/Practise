using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Vicuna.Storage.Extensions;
using Vicuna.Storage.Paging;
using Vicuna.Storage.Transactions;

namespace Vicuna.Storage.Data.Trees
{
    public unsafe class TreePage : PageAccessor
    {
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

        public int LastMatch { get; set; }

        public int LastMatchIndex { get; set; }

        public TreeNodeDataSlice MinKey
        {
            get => GetNodeKey(0);
        }

        public TreeNodeDataSlice MaxKey
        {
            get => GetNodeKey(Header.ItemCount - (IsLeaf ? 1 : 2));
        }

        public ref TreeNodeHeader this[int index]
        {
            get
            {
                return ref GetNodeHeader(GetNodePos(index));
            }
        }

        public ref TreePageHeader Header
        {
            get => ref Read<TreePageHeader>(0);
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
                Compact();
                upper = (ushort)(Header.Upper - size);
            }

            if (index <= Header.ItemCount - 1)
            {
                //move index region
                var moveStart = GetIndexPos(index);
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

        public void InsertDataNode(int index, ushort pos, TreeNodeDataEntry entry, long txNumber)
        {
            ref var node = ref GetNodeHeader(pos);
            ref var tx = ref GetNodeTransactionHeader((ushort)(pos + entry.Key.Size + TreeNodeHeader.SizeOf));

            var key = Slice(pos + TreeNodeHeader.SizeOf, entry.Key.Size);
            var value = Slice(pos + TreeNodeHeader.SizeOf + TreeNodeTransactionHeader.SizeOf + entry.Key.Size, entry.Value.Size);

            node.KeySize = (ushort)entry.Key.Size;
            node.DataSize = (uint)entry.Value.Size;
            node.IsDeleted = false;
            node.NodeFlags = TreeNodeHeaderFlags.Data;

            tx.TransactionNumber = txNumber;
            tx.TransactionRollbackNumber = -1;

            entry.Key.CopyTo(key);
            entry.Value.CopyTo(value);
        }

        public void InsertDataRefNode(int index, ushort pos, TreeNodeDataEntry entry)
        {
            ref var node = ref GetNodeHeader(pos);

            var key = Slice(pos + TreeNodeHeader.SizeOf, entry.Key.Size);
            var value = Slice(pos + TreeNodeHeader.SizeOf + entry.Key.Size, entry.Value.Size);

            node.KeySize = (ushort)entry.Key.Size;
            node.DataSize = (uint)entry.Value.Size;
            node.IsDeleted = false;
            node.NodeFlags = TreeNodeHeaderFlags.Data;

            entry.Key.CopyTo(key);
            entry.Value.CopyTo(value);
        }

        public void InsertPageRefNode(int index, ushort pos, TreeNodeDataSlice keySlice, long pageNumber)
        {
            ref var node = ref GetNodeHeader(pos);
            var key = Slice(pos + TreeNodeHeader.SizeOf, keySlice.Size);

            node.KeySize = (ushort)keySlice.Size;
            node.PageNumber = pageNumber;
            node.IsDeleted = false;
            node.NodeFlags = TreeNodeHeaderFlags.PageRef;

            keySlice.CopyTo(key);
        }

        public void RemoveNode(int index, long txNumber, long txLogNumber)
        {
            var npos = GetNodePos(index);
            var ipos = GetIndexPos(index);

            // move index region
            var start = (index + 1) * sizeof(ushort) + Constants.PageHeaderSize;
            var size = Header.Low - start;

            var to = Slice(ipos, size);
            var from = Slice(start, size);

            from.CopyTo(to);
            Write(Header.Low, (ushort)0);

            //tx info
            ref var node = ref GetNodeHeader(npos);
            if (node.NodeFlags == TreeNodeHeaderFlags.Data)
            {
                ref var tx = ref GetNodeTransactionHeader((ushort)(npos + node.KeySize + TreeNodeTransactionHeader.SizeOf));

                tx.TransactionNumber = txNumber;
                tx.TransactionRollbackNumber = txLogNumber;
            }

            node.IsDeleted = true;
        }

        public void RemoveNode(int index)
        {
            var npos = GetNodePos(index);
            var ipos = GetIndexPos(index);
            ref var node = ref GetNodeHeader(npos);

            // move index region
            var start = (index + 1) * sizeof(ushort) + Constants.PageHeaderSize;
            var size = Header.Low - start;

            var to = Slice(ipos, size);
            var from = Slice(start, size);

            from.CopyTo(to);
            Write(Header.Low, (ushort)0);

            Header.Low -= sizeof(ushort);
            Header.ItemCount--;
            Header.UsedLength -= node.GetSize();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ref TreeNodeHeader GetNodeHeader(ushort pos)
        {
            return ref Read<TreeNodeHeader>(pos, TreeNodeHeader.SizeOf);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ref TreeNodeHeader GetLastMatchNodeHeader()
        {
            var pos = GetNodePos(LastMatchIndex);

            return ref Read<TreeNodeHeader>(pos, TreeNodeHeader.SizeOf);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ref TreeNodeTransactionHeader GetNodeTransactionHeader(ushort txPos)
        {
            return ref Read<TreeNodeTransactionHeader>(txPos, TreeNodeTransactionHeader.SizeOf);
        }

        public TreeNodeDataSlice GetNodeKey(int index)
        {
            var pos = GetNodePos(index);
            var node = GetNodeHeader(pos);
            var keyPos = GetNodeKeyPos(ref node, pos);
            var key = Slice(keyPos, node.KeySize);
            if (key.Length == 0)
            {
                throw new InvalidOperationException($"the node data slice is empty,index:{index}!");
            }

            return new TreeNodeDataSlice(key, TreeNodeDataSliceType.Key);
        }

        public TreeNodeDataSlice GetNodeData(int index)
        {
            var pos = GetNodePos(index);
            var node = GetNodeHeader(pos);
            if (node.NodeFlags == TreeNodeHeaderFlags.PageRef)
            {
                throw new InvalidOperationException($"node flags :{node.NodeFlags} no data:{index}");
            }

            var dataPos = GetNodeDataPos(ref node, pos);
            var data = Slice(dataPos, (int)node.DataSize);
            if (data.Length == 0)
            {
                throw new InvalidOperationException($"the node data slice is empty,index:{index}!");
            }

            return new TreeNodeDataSlice(data, TreeNodeDataSliceType.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort GetNodePos(int index)
        {
            return Read<ushort>(GetIndexPos(index));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort GetIndexPos(int index)
        {
            return (ushort)(index * sizeof(ushort) + Constants.PageHeaderSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort GetNodeKeyPos(ref TreeNodeHeader node, ushort pos)
        {
            return (ushort)(pos + TreeNodeHeader.SizeOf);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort GetNodeDataPos(ref TreeNodeHeader node, ushort pos)
        {
            switch (node.NodeFlags)
            {
                case TreeNodeHeaderFlags.Data:
                    return (ushort)(TreeNodeHeader.SizeOf + pos + node.KeySize + TreeNodeTransactionHeader.SizeOf);
                default:
                    return (ushort)(TreeNodeHeader.SizeOf + pos + node.KeySize);
            }
        }


        private void Compact()
        {
            var count = Header.ItemCount;
            var length = Constants.PageSize - Header.Upper;
            var index = length;
            var buffer = new Span<byte>(new byte[length]);

            for (var i = 0; i < count; i++)
            {
                var nIndex = GetIndexPos(i);
                var nOffset = GetNodePos(i);
                ref var node = ref GetNodeHeader(nOffset);
                var size = node.GetSize() - sizeof(ushort);

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
        }

        public void Search(TreeNodeDataSlice key)
        {
            var count = IsLeaf ? Header.ItemCount : Header.ItemCount - 1;
            if (count <= 0)
            {
                LastMatch = 1;
                LastMatchIndex = 0;
                return;
            }

            if (IsLessThanOrEqualMinKey(key, count) ||
                IsMoreThanOrEqualMaxKey(key, count))
            {
                return;
            }

            BinarySearch(key, 0, count - 1);
        }

        public bool SearchPageIfBranch(ILowLevelTransaction tx, TreeNodeDataSlice key, out TreePage page)
        {
            if (IsLeaf)
            {
                page = null;
                return false;
            }

            Search(key);

            if (LastMatch != 0)
            {
                page = null;
                return false;
            }

            var node = GetLastMatchNodeHeader();
            var data = tx.GetPage(Header.StoreId, node.PageNumber);
            if (data == null)
            {
                throw null;
            }

            page = data.AsTree();
            return true;
        }

        public void BinarySearch(TreeNodeDataSlice key, int first, int last)
        {
            while (first < last)
            {
                var mid = first + (last - first) / 2;
                var midKey = GetNodeKey(mid);
                var flag = CompareTo(midKey, key);
                if (flag == 0)
                {
                    LastMatch = 0;
                    LastMatchIndex = IsLeaf ? mid : mid + 1;
                    return;
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
                LastMatch = 0;
                LastMatchIndex = IsLeaf ? last : last + 1;
                return;
            }

            LastMatch = IsBranch ? 0 : lastFlag;
            LastMatchIndex = last;
        }

        private bool IsLessThanOrEqualMinKey(TreeNodeDataSlice key, int count)
        {
            //<=first
            var flag = CompareTo(MinKey, key);
            if (flag >= 0)
            {
                LastMatch = IsBranch ? 0 : flag;
                LastMatchIndex = IsBranch && flag == 0 ? 1 : 0;
                return true;
            }

            return false;
        }

        private bool IsMoreThanOrEqualMaxKey(TreeNodeDataSlice key, int count)
        {
            //>=last 
            var flag = CompareTo(MaxKey, key);
            if (flag <= 0)
            {
                LastMatch = IsBranch ? 0 : flag;
                LastMatchIndex = IsBranch ? count : count - 1;
                return true;
            }

            return false;
        }

        internal int CopyEntriesToNewPage(int index, TreePage newPage)
        {
            var min = 1;
            var count = Header.ItemCount;
            var start = index < min ? min : index;

            for (var i = count - 1; i >= index; i--)
            {
                if (CopyEntryToNewPage(i, 0, newPage, CopyNodeEntryType.SourcePageSpaceFirst, out var size))
                {
                    Header.Low -= sizeof(ushort);
                    Header.UsedLength -= size;
                    Header.ItemCount--;
                    continue;
                }

                start = i + 1;
                break;
            }

            if (start == index)
            {
                while (start - 1 > min && CopyEntryToNewPage(start - 1, 0, newPage, CopyNodeEntryType.TargetMoreSpaceFirst, out var size))
                {
                    Header.Low -= sizeof(ushort);
                    Header.UsedLength -= size;
                    Header.ItemCount--;
                    start--;
                }
            }

            return start;
        }

        internal bool CopyEntryToNewPage(int sourceIndex, int destIndex, TreePage newPage, CopyNodeEntryType copyType, out ushort nodeSize)
        {
            if (sourceIndex > Header.ItemCount - 1)
            {
                nodeSize = 0;
                return false;
            }

            var pos = GetNodePos(sourceIndex);
            var node = GetNodeHeader(pos);
            var size = node.GetSize();

            switch (copyType)
            {
                case CopyNodeEntryType.SourcePageSpaceFirst:
                    if (newPage.Header.UsedLength + size >= Header.UsedLength - size)
                    {
                        nodeSize = 0;
                        return false;
                    }
                    break;
                case CopyNodeEntryType.TargetMoreSpaceFirst:
                    if (newPage.Header.UsedLength + size <= Header.UsedLength - size)
                    {
                        nodeSize = 0;
                        return false;
                    }
                    break;
            }

            if (!newPage.AllocateInternal(destIndex, (ushort)(size - sizeof(ushort)), node.NodeFlags, out var newNodeOffset))
            {
                throw new Exception("tree page split failed!");
            }

            var oldNode = Slice(pos, size - sizeof(ushort));
            var newNode = newPage.Slice(newNodeOffset, size - sizeof(ushort));

            oldNode.CopyTo(newNode);
            nodeSize = size;
            return true;
        }

        public void CopyTo(TreePage page)
        {
            var header = page.Header;

            Array.Copy(Data, page.Data, Data.Length);

            page.Header.StoreId = header.StoreId;
            page.Header.PageNumber = header.PageNumber;
            page.LastMatch = LastMatch;
            page.LastMatchIndex = LastMatchIndex;
        }

        public void Clear()
        {
            ref var header = ref Header;

            header.ItemCount = 0;
            header.Low = Constants.PageHeaderSize;
            header.Upper = Constants.PageSize;
            header.UsedLength = Constants.PageHeaderSize;

            LastMatch = 0;
            LastMatchIndex = 0;

            Array.Clear(Data, Constants.PageHeaderSize, Constants.PageSize - Constants.PageHeaderSize);
        }

        public static int CompareTo(TreeNodeDataSlice left, TreeNodeDataSlice right)
        {
            var index = 0;

            while (index < left.Size)
            {
                var match = 0;
                var type = (DataValueType)left[index];

                index++;

                switch (type)
                {
                    case DataValueType.Char:
                        match = left.GetChar(index) - right.GetChar(index);
                        index += sizeof(char);
                        break;
                    case DataValueType.Byte:
                    case DataValueType.Boolean:
                        match = left.GetByte(index) - right.GetByte(index);
                        index += sizeof(byte);
                        break;
                    case DataValueType.Int16:
                        match = left.GetInt16(index) - right.GetInt16(index);
                        index += sizeof(short);
                        break;
                    case DataValueType.Int32:
                        match = left.GetInt32(index) - right.GetInt32(index);
                        index += sizeof(int);
                        break;
                    case DataValueType.UInt16:
                        match = left.GetUInt16(index) - right.GetUInt16(index);
                        index += sizeof(ushort);
                        break;
                    case DataValueType.Int64:
                    case DataValueType.DateTime:
                        match = left.GetInt64(index).CompareTo(right.GetInt64(index));
                        index += sizeof(long);
                        break;
                    case DataValueType.UInt32:
                        match = left.GetUInt32(index).CompareTo(right.GetUInt32(index));
                        index += sizeof(uint);
                        break;
                    case DataValueType.UInt64:
                        match = left.GetUInt64(index).CompareTo(right.GetUInt64(index));
                        index += sizeof(ulong);
                        break;
                    case DataValueType.Single:
                        match = left.GetSingle(index).CompareTo(right.GetSingle(index));
                        index += sizeof(float);
                        break;
                    case DataValueType.Double:
                        match = left.GetDouble(index).CompareTo(right.GetDouble(index));
                        index += sizeof(double);
                        break;
                    case DataValueType.String:
                        var s1 = left.GetSpanString(index);
                        var s2 = right.GetSpanString(index);

                        fixed (byte* p1 = s1, p2 = s2)
                        {
                            match = CompareTo(p1, p2, s1.Length, s2.Length);
                        }

                        index += left[index];
                        break;
                }

                if (match != 0)
                {
                    return match;
                }
            }

            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CompareTo(byte* p1, byte* p2, int len1, int len2)
        {
            var lp1 = p1;
            var lp2 = p2;
            var len = Math.Min(len1, len2);

            for (var i = 0; i < len / 8; i++)
            {
                if (*(long*)lp1 != *(long*)lp2)
                {
                    if (*(int*)lp1 == *(int*)lp2)
                    {
                        lp1 += 4;
                        lp2 += 4;
                    }

                    return CompareTo(lp1, lp2, sizeof(int));
                }

                lp1 += 8;
                lp2 += 8;
            }

            if ((len & 0x04) != 0)
            {
                if (*(int*)lp1 != *(int*)lp2)
                {
                    return CompareTo(lp1, lp2, sizeof(int));
                }

                lp1 += 4;
                lp2 += 4;
            }

            if ((len & 0x02) != 0)
            {
                if (*(short*)lp1 != *(short*)lp2)
                {
                    return CompareTo(lp1, lp2, sizeof(short));
                }

                lp1 += 2;
                lp2 += 2;
            }

            if ((len & 0x01) != 0)
            {
                var flag = *lp1 - *lp2;
                if (flag != 0)
                {
                    return flag;
                }
            }

            return len1 - len2;
        }

        public static int CompareTo(byte* p1, byte* p2, int len)
        {
            for (var n = 0; n < len; n++)
            {
                var flag = p1[n] - p2[n];
                if (flag != 0)
                {
                    return flag;
                }
            }

            return 0;
        }

        public enum CopyNodeEntryType
        {
            SourcePageSpaceFirst,

            TargetMoreSpaceFirst,
        }

        public List<byte[]> Keys
        {
            get
            {
                var keys = new List<byte[]>();

                for (var i = 0; i < Header.ItemCount - (IsBranch ? 1 : 0); i++)
                {
                    var key = GetNodeKey(i);
                    if (key.Size > 0)
                    {
                        keys.Add(key.Data.ToArray());
                    }
                    else
                    {
                        keys.Add(new byte[0]);
                    }
                }

                return keys;
            }
        }

        internal List<TreeNodeHeader> Nodes
        {
            get
            {
                var nodes = new List<TreeNodeHeader>();

                for (var i = 0; i < Header.ItemCount; i++)
                {
                    var offset = GetNodePos(i);
                    var node = GetNodeHeader(offset);

                    nodes.Add(node);
                }

                return nodes;
            }
        }

        internal bool Sorted
        {
            get
            {
                var keys = Keys;

                for (var i = 0; i < keys.Count - 1; i++)
                {
                    var key1 = new TreeNodeDataSlice(keys[i], TreeNodeDataSliceType.Key);
                    var key2 = new TreeNodeDataSlice(keys[i + 1], TreeNodeDataSliceType.Key);
                    if (CompareTo(key1, key2) > 0)
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }

    public ref struct TreeNodeKey
    {
        public ushort Size;

        public Span<byte> Keys;

        public TreeNodeKey(Span<byte> keys)
        {
            Keys = keys;
            Size = (ushort)keys.Length;
        }

        public void CopyTo(Span<byte> dest)
        {
            Keys.CopyTo(dest);
        }

        public override string ToString()
        {
            var str = "";

            for (var i = 1; i < Size; i++)
            {
                str += (char)Keys[i];
            }

            return str;
        }
    }

    public ref struct TreeNodeDataSlice
    {
        public int Size { get; }

        public Span<byte> Data { get; }

        public TreeNodeDataSliceType Type { get; }

        public ref byte this[int index] => ref Data[index];

        public TreeNodeDataSlice(Span<byte> data, TreeNodeDataSliceType type)
        {
            Data = data;
            Type = type;
            Size = data.Length;
        }

        public void CopyTo(Span<byte> dest)
        {
            Data.CopyTo(dest);
        }

        public override string ToString()
        {
            var str = "";

            for (var i = 2; i < Size; i++)
            {
                str += (char)Data[i];
            }

            return str;
        }

        public ref byte GetByte(int index)
        {
            if (index < 0 || index + sizeof(byte) > Size)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return ref Data[index];
        }

        public ref char GetChar(int index)
        {
            if (index < 0 || index + sizeof(char) > Size)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return ref Unsafe.As<byte, char>(ref Data[index]);
        }

        public ref short GetInt16(int index)
        {
            if (index < 0 || index + sizeof(short) > Size)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return ref Unsafe.As<byte, short>(ref Data[index]);
        }

        public ref ushort GetUInt16(int index)
        {
            if (index < 0 || index + sizeof(ushort) > Size)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return ref Unsafe.As<byte, ushort>(ref Data[index]);
        }

        public ref int GetInt32(int index)
        {
            if (index < 0 || index + sizeof(int) > Size)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return ref Unsafe.As<byte, int>(ref Data[index]);
        }

        public ref uint GetUInt32(int index)
        {
            if (index < 0 || index + sizeof(uint) > Size)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return ref Unsafe.As<byte, uint>(ref Data[index]);
        }

        public ref long GetInt64(int index)
        {
            if (index < 0 || index + sizeof(long) > Size)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return ref Unsafe.As<byte, long>(ref Data[index]);
        }

        public ref ulong GetUInt64(int index)
        {
            if (index < 0 || index + sizeof(ulong) > Size)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return ref Unsafe.As<byte, ulong>(ref Data[index]);
        }

        public ref float GetSingle(int index)
        {
            if (index < 0 || index + sizeof(float) > Size)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return ref Unsafe.As<byte, float>(ref Data[index]);
        }

        public ref double GetDouble(int index)
        {
            if (index < 0 || index + sizeof(double) > Size)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return ref Unsafe.As<byte, double>(ref Data[index]);
        }

        public string GetString(int index)
        {
            if (index < 0 || index + Data[index] > Size)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return System.Text.Encoding.UTF8.GetString(Data.Slice(index + 1, Data[index]));
        }

        public Span<byte> GetSpanString(int index)
        {
            if (index < 0 || index + Data[index] > Size)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return Data.Slice(index + 1, Data[index]);
        }

        public ref bool GetBoolean(int index)
        {
            if (index < 0 || index + sizeof(bool) > Size)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return ref Unsafe.As<byte, bool>(ref Data[index]);
        }
    }

    public enum TreeNodeDataSliceType
    {
        Key,

        Value
    }

    public ref struct TreeNodeDataEntry
    {
        public TreeNodeDataSlice Key;

        public TreeNodeDataSlice Value;

        public int Size => Key.Size + Value.Size;
    }
}