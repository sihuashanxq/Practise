using System;
using Vicuna.Storage.Data.Tables;
using Vicuna.Storage.Extensions;
using Vicuna.Storage.Paging;
using Vicuna.Storage.Transactions;

namespace Vicuna.Storage.Data.Trees
{
    public class Tree
    {
        public int Id { get; }

        public TreeState State { get; }

        public TreePage _root;

        private readonly bool _isUnique = false;

        public const ushort PageEntryMaxDataSize = (Constants.PageSize - Constants.PageHeaderSize) / 2 - TreeNodeHeader.SizeOf - TreeNodeTransactionHeader.SizeOf;

        public Tree()
        {
            State = new TreeState(new TreeRootHeader());
        }

        public Tree(TreeRootHeader header)
        {
            State = new TreeState(header);
        }

        public void Init(ILowLevelTransaction tx)
        {
            if (_root == null)
            {
                _root = AllocateEntry(tx, TreeNodeFlags.Leaf, null, 0).Page;
            }
        }

        public TreeCursor Get(ILowLevelTransaction tx, Span<byte> key)
        {
            return BuildCursor(tx, key);
        }

        public Span<byte> GetValue(ILowLevelTransaction tx, Span<byte> key)
        {
            var cursor = BuildCursor(tx, key);
            if (cursor != null && cursor.Entry != null && cursor.Entry.Page != null)
            {
                cursor.Entry.Search(key);

                return cursor.Entry.GetNodeData(cursor.Entry.LastMatchIndex);
            }

            return Span<byte>.Empty;
        }

        public void AddEntry(ILowLevelTransaction tx, EncodingByteString key, EncodingByteString value)
        {
            AddEntry(tx, key.Values, value.Values);
        }

        public void AddEntry(ILowLevelTransaction tx, ValueEncodingByteString key, ValueEncodingByteString value)
        {
            AddEntry(tx, key.Values, value.Values);
        }

        public void AddEntry(ILowLevelTransaction tx, Span<byte> key, Span<byte> value)
        {

        }

        public void RemoveEntry(ILowLevelTransaction tx, EncodingByteString key)
        {
            RemoveEntry(tx, key.Values);
        }

        public void RemoveEntry(ILowLevelTransaction tx, ValueEncodingByteString key)
        {
            RemoveEntry(tx, key.Values);
        }

        public void RemoveEntry(ILowLevelTransaction tx, Span<byte> key)
        {

        }

        public void CleanEntry(ILowLevelTransaction tx, EncodingByteString key)
        {
            CleanEntry(tx, key.Values);
        }

        public void CleanEntry(ILowLevelTransaction tx, ValueEncodingByteString key)
        {
            CleanEntry(tx, key.Values);
        }

        public void CleanEntry(ILowLevelTransaction tx, Span<byte> key)
        {

        }

        //public unsafe void Insert(ILowLevelTransaction tx, TreeNodeDataEntry data)
        //{
        //    var cursor = BuildCursor(tx, data.Key);
        //    var match = SearchForKey(cursor.Entry, data.Key);
        //    if (match == 0 && _isUnique)
        //    {
        //        throw new InvalidCastException($"mulpti key");
        //    }

        //    if (!ModifyEntry(cursor.Entry, tx))
        //    {
        //        throw new InvalidOperationException($"modify page failed!");
        //    }

        //    Insert(tx, cursor.Entry, data);
        //}

        //private unsafe void Insert(ILowLevelTransaction tx, TreePageEntry entry, TreeNodeDataEntry data)
        //{
        //    var size = data.Size;
        //    if (size >= PageEntryMaxDataSize)
        //    {
        //        InsertOverflow(tx, entry, data);
        //        return;
        //    }

        //    if (entry.Allocate(entry.LastMatchIndex, (ushort)size, TreeNodeHeaderFlags.Data, out var pos))
        //    {
        //        entry.InsertDataNode(entry.LastMatchIndex, pos, data, 0l);
        //        return;
        //    }

        //    var index = Balance(tx, entry, data.Key, entry.LastMatchIndex);
        //    if (index <= entry.LastMatchIndex)
        //    {
        //        //add to new right page
        //        entry.LastMatchIndex = entry.LastMatchIndex - index;
        //    }

        //    if (!entry.Allocate(entry.LastMatchIndex, (ushort)size, TreeNodeHeaderFlags.Data, out pos))
        //    {
        //        throw new InvalidOperationException("allocate node space faild!");
        //    }

        //    entry.InsertDataNode(entry.LastMatchIndex, pos, data, 0l);
        //}

        //public void InsertOverflow(ILowLevelTransaction tx, TreePageEntry entry, TreeNodeDataEntry data)
        //{
        //    var size = data.Key.Size;
        //    var dataSize = data.Value.Size;
        //    var overflowPages = AllocateOverflows(tx, dataSize);

        //    for (var i = 0; i < overflowPages.Length; i++)
        //    {
        //        var pageSize = Math.Min(OverflowPage.Capacity, dataSize);

        //        overflowPages[i].Write(Constants.PageHeaderSize, data.Value.Data.Slice(i * OverflowPage.Capacity, pageSize));

        //        dataSize -= pageSize;
        //    }

        //    if (entry.Allocate(entry.LastMatchIndex, (ushort)size, TreeNodeHeaderFlags.Data, out var pos))
        //    {
        //        entry.InsertPageRefNode(entry.LastMatchIndex, pos, data.Key, overflowPages[0].Header.PageNumber);
        //        return;
        //    }

        //    var index = Balance(tx, entry, data.Key, entry.LastMatchIndex);
        //    if (index <= entry.LastMatchIndex)
        //    {
        //        //add to new right page
        //        entry.LastMatchIndex = entry.LastMatchIndex - index;
        //    }

        //    if (!entry.Allocate(entry.LastMatchIndex, (ushort)size, TreeNodeHeaderFlags.Data, out pos))
        //    {
        //        throw new InvalidOperationException("allocate node space faild!");
        //    }

        //    entry.InsertPageRefNode(entry.LastMatchIndex, pos, data.Key, overflowPages[0].Header.PageNumber);
        //}

        //public unsafe int Balance(ILowLevelTransaction tx, TreePageEntry entry, TreeNodeDataSlice key, int midIndex)
        //{
        //    var newEntry = AllocateEntry(tx, entry.Header.NodeFlags, entry.Parent, entry.Index + 1);
        //    if (newEntry == null)
        //    {
        //        throw new InvalidOperationException("allocate new page failed!");
        //    }

        //    var balanceIndex = entry.CopyEntriesToNewPage(midIndex, newEntry.Page);
        //    if (balanceIndex > midIndex)
        //    {
        //        Balance(tx, entry, newEntry, newEntry.MinKey);

        //        return balanceIndex;
        //    }

        //    var index = midIndex - balanceIndex;
        //    var newKey = index > 0 ? newEntry.MinKey : key;

        //    Balance(tx, entry, newEntry, newKey);

        //    entry.Page = newEntry.Page;
        //    entry.Index = newEntry.Index;

        //    return balanceIndex;
        //}

        //public unsafe void Balance(ILowLevelTransaction tx, TreePageEntry curEntry, TreePageEntry newEntry, TreeNodeDataSlice key)
        //{
        //    var parent = curEntry.Parent;
        //    if (parent != null && parent.Page != null)
        //    {
        //        ModifyEntry(parent, tx);
        //        Balance(tx, parent, curEntry, newEntry, key);
        //        return;
        //    }

        //    var entry = AllocateEntry(tx, curEntry.Header.NodeFlags, null, curEntry.Index);
        //    if (entry == null)
        //    {
        //        throw new NullReferenceException(nameof(entry));
        //    }

        //    parent = new TreePageEntry(curEntry.Page, 0, null);

        //    newEntry.Parent = parent;
        //    curEntry.Parent = parent;
        //    curEntry.Page = entry.Page;
        //    curEntry.Index = entry.Index;

        //    parent.Page.CopyTo(entry.Page);
        //    parent.Page.Clear();
        //    parent.Page.Header.NodeFlags = TreeNodeFlags.Branch;

        //    Balance(tx, parent, curEntry, newEntry, key);
        //}

        //public unsafe void Balance(ILowLevelTransaction tx, TreePageEntry parent, TreePageEntry curEntry, TreePageEntry newEntry, TreeNodeDataSlice key)
        //{
        //    var index = curEntry.Index;
        //    if (parent.Page.Header.ItemCount == 0)
        //    {
        //        curEntry.Index = 0;
        //        newEntry.Index = 1;

        //        parent.Allocate(0, (ushort)key.Size, TreeNodeHeaderFlags.PageRef, out var curPos);
        //        parent.Page.Allocate(1, 0, TreeNodeHeaderFlags.PageRef, out var newPos);

        //        parent.InsertPageRefNode(0, curPos, key, curEntry.Header.PageNumber);
        //        parent.InsertPageRefNode(1, newPos, new TreeNodeDataSlice(), newEntry.Header.PageNumber);
        //        return;
        //    }

        //    if (!parent.Allocate(index, (ushort)key.Size, TreeNodeHeaderFlags.PageRef, out var pos))
        //    {
        //        var halfIndex = parent.Header.ItemCount / 2;
        //        var halfKey = parent.GetNodeKey(halfIndex - 1);
        //        var balanceIndex = Balance(tx, parent, halfKey, halfIndex);
        //        if (balanceIndex <= halfIndex && balanceIndex <= curEntry.Index)
        //        {
        //            index = curEntry.Index - balanceIndex;
        //            newEntry.Index = index + 1;
        //        }
        //        else if (balanceIndex > halfIndex && balanceIndex < curEntry.Index)
        //        {
        //            index = curEntry.Index - balanceIndex - 1;
        //            newEntry.Index = index + 1;
        //        }

        //        if (!parent.Page.Allocate(index, (ushort)key.Size, TreeNodeHeaderFlags.PageRef, out pos))
        //        {
        //            throw new InvalidOperationException($"balance page:{parent.Page.Header.PageNumber} failed!");
        //        }
        //    }

        //    parent.Page[index + 1].PageNumber = newEntry.Header.PageNumber;
        //    parent.Page.InsertPageRefNode(index, pos, key, curEntry.Header.PageNumber);
        //}

        private TreeCursor BuildCursor(ILowLevelTransaction tx, Span<byte> key)
        {
            if (_root == null)
            {
                _root = tx.GetPage(0, 0).AsTree();
            }

            var entry = new TreePageEntry(_root, 0, null);

            while (entry.SearchPageIfBranch(tx, key, out var page))
            {
                entry = new TreePageEntry(page, entry.LastMatchIndex, entry);
            }

            return new TreeCursor(new Memory<byte>(key.ToArray()), tx, entry);
        }

        private int SearchForKey(TreePageEntry entry, Span<byte> key)
        {
            entry.Search(key);

            if (entry.LastMatch < 0)
            {
                entry.LastMatchIndex++;
            }

            return entry.LastMatch;
        }

        private bool ModifyEntry(TreePageEntry entry, ILowLevelTransaction tx)
        {
            var lastMatch = entry.LastMatch;
            var lastMatchIndex = entry.LastMatchIndex;

            var newPage = tx.ModifyPage(entry.Page);
            if (newPage == null)
            {
                return false;
            }

            entry.Page = newPage;
            entry.LastMatch = lastMatch;
            entry.LastMatchIndex = lastMatchIndex;

            State.OnChanged(entry.Page, TreePageChangeFlags.Modify);
            return true;
        }

        private unsafe TreePageEntry AllocateEntry(ILowLevelTransaction tx, TreeNodeFlags nodeFlags, TreePageEntry parent, int index)
        {
            var page = tx.AllocateTrees(Id, 1)[0];

            ref var header = ref page.Header;

            header.ItemCount = 0;
            header.Flags = PageHeaderFlags.Tree;
            header.NodeFlags = nodeFlags;
            header.Low = Constants.PageHeaderSize;
            header.Upper = Constants.PageSize;
            header.UsedLength = Constants.PageHeaderSize;

            State.OnChanged(page, TreePageChangeFlags.New);

            return new TreePageEntry(page, index, parent);
        }

        private OverflowPage[] AllocateOverflows(ILowLevelTransaction tx, int dataSize)
        {
            var count = dataSize % OverflowPage.Capacity == 0
               ? dataSize / OverflowPage.Capacity
               : dataSize / OverflowPage.Capacity + 1;

            var overflows = tx.AllocateOverflows(Id, (uint)count);

            foreach (var overflow in overflows)
            {
                State.OnChanged(overflow, TreePageChangeFlags.New);
            }

            return overflows;
        }
    }
}
