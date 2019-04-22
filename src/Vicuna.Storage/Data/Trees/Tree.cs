using System;
using Vicuna.Storage.Extensions;
using Vicuna.Storage.Paging;
using Vicuna.Storage.Transactions;

namespace Vicuna.Storage.Data.Trees
{
    public class Tree
    {
        public TreePage _root;

        private readonly bool _isUnique = false;

        public const ushort MaxPageDataSize = (Constants.PageSize - Constants.PageHeaderSize) / 2 - TreeNodeHeader.SizeOf - TreeNodeTransactionHeader.SizeOf;

        public void Init(ILowLevelTransaction tx)
        {
            if (_root == null)
            {
                _root = AllocateEntry(tx, TreeNodeFlags.Leaf, null, 0).Page;
            }
        }

        public TreeCursor Get(TreeNodeDataSlice key, ILowLevelTransaction tx)
        {
            return BuildCursorForKey(key, tx);
        }

        public TreeNodeDataSlice GetValue(TreeNodeDataSlice key, ILowLevelTransaction tx)
        {
            var cursor = BuildCursorForKey(key, tx);
            if (cursor != null && cursor.Entry != null && cursor.Entry.Page != null)
            {
                cursor.Entry.Page.Search(key);

                return cursor.Entry.Page.GetNodeData(cursor.Entry.Page.LastMatchIndex);
            }

            return new TreeNodeDataSlice();
        }

        public unsafe void Insert(TreeNodeDataEntry data, ILowLevelTransaction tx)
        {
            Init(tx);

            var cursor = BuildCursorForKey(data.Key, tx);
            if (!cursor.Entry.Page.IsLeaf)
            {
                throw new InvalidOperationException("not a leaf page!");
            }

            var match = SearchKeyForModify(cursor.Entry, data.Key);
            if (match == 0 && _isUnique)
            {
                throw new InvalidCastException($"mulpti key");
            }

            if (!ModifyEntry(cursor.Entry, tx))
            {
                throw new InvalidOperationException($"modify page failed!");
            }

            var size = data.Size;
            var overflowSize = 0;
            if (size > MaxPageDataSize)
            {
                size = data.Key.Size;
                overflowSize = data.Value.Size;
            }

            var entry = cursor.Entry;
            if (!entry.Page.Allocate(entry.Page.LastMatchIndex, (ushort)size, TreeNodeHeaderFlags.Data, out var pos))
            {
                var balanceIndex = Balance(tx, entry, data.Key, entry.Page.LastMatchIndex);
                if (balanceIndex <= entry.Page.LastMatchIndex)
                {
                    //add to new right page
                    entry.Page.LastMatchIndex = entry.Page.LastMatchIndex - balanceIndex;
                }

                if (!entry.Page.Allocate(entry.Page.LastMatchIndex, (ushort)size, TreeNodeHeaderFlags.Data, out pos))
                {
                    throw new InvalidOperationException("allocate node space faild!");
                }
            }

            entry.Page.InsertDataNode(entry.Page.LastMatchIndex, pos, data, 0l);
        }

        public unsafe int Balance(ILowLevelTransaction tx, TreePageEntry entry, TreeNodeDataSlice key, int midIndex)
        {
            var newEntry = AllocateEntry(tx, entry.Page.Header.NodeFlags, entry.Parent, entry.Index + 1);
            if (newEntry == null)
            {
                throw new InvalidOperationException("allocate new page failed!");
            }

            var balanceIndex = entry.Page.CopyEntriesToNewPage(midIndex, newEntry.Page);
            if (balanceIndex > midIndex)
            {
                Balance(tx, entry, newEntry, newEntry.Page.MinKey);

                return balanceIndex;
            }

            var index = midIndex - balanceIndex;
            var newKey = index > 0 ? newEntry.Page.MinKey : key;

            Balance(tx, entry, newEntry, newKey);

            entry.Page = newEntry.Page;
            entry.Index = newEntry.Index;

            return balanceIndex;
        }

        public unsafe void Balance(ILowLevelTransaction tx, TreePageEntry curEntry, TreePageEntry newEntry, TreeNodeDataSlice key)
        {
            var parent = curEntry.Parent;
            if (parent != null && parent.Page != null)
            {
                ModifyEntry(parent, tx);
                Balance(tx, parent, curEntry, newEntry, key);
                return;
            }

            var entry = AllocateEntry(tx, curEntry.Page.Header.NodeFlags, null, curEntry.Index);
            if (entry == null)
            {
                throw new NullReferenceException(nameof(entry));
            }

            parent = new TreePageEntry(curEntry.Page, 0, null);

            newEntry.Parent = parent;
            curEntry.Parent = parent;
            curEntry.Page = entry.Page;
            curEntry.Index = entry.Index;

            parent.Page.CopyTo(entry.Page);
            parent.Page.Clear();
            parent.Page.Header.NodeFlags = TreeNodeFlags.Branch;

            Balance(tx, parent, curEntry, newEntry, key);
        }

        public unsafe void Balance(ILowLevelTransaction tx, TreePageEntry parent, TreePageEntry curEntry, TreePageEntry newEntry, TreeNodeDataSlice key)
        {
            var index = curEntry.Index;
            if (parent.Page.Header.ItemCount == 0)
            {
                curEntry.Index = 0;
                newEntry.Index = 1;

                parent.Page.Allocate(0, (ushort)key.Size, TreeNodeHeaderFlags.PageRef, out var curPos);
                parent.Page.Allocate(1, 0, TreeNodeHeaderFlags.PageRef, out var newPos);

                parent.Page.InsertPageRefNode(0, curPos, key, curEntry.Page.Header.PageNumber);
                parent.Page.InsertPageRefNode(1, newPos, new TreeNodeDataSlice(), newEntry.Page.Header.PageNumber);
                return;
            }

            if (!parent.Page.Allocate(index, (ushort)key.Size, TreeNodeHeaderFlags.PageRef, out var pos))
            {
                var halfIndex = parent.Page.Header.ItemCount / 2;
                var halfKey = parent.Page.GetNodeKey(halfIndex - 1);
                var balanceIndex = Balance(tx, parent, halfKey, halfIndex);
                if (balanceIndex <= halfIndex && balanceIndex <= curEntry.Index)
                {
                    index = curEntry.Index - balanceIndex;
                    newEntry.Index = index + 1;
                }
                else if (balanceIndex > halfIndex && balanceIndex < curEntry.Index)
                {
                    index = curEntry.Index - balanceIndex - 1;
                    newEntry.Index = index + 1;
                }

                if (!parent.Page.Allocate(index, (ushort)key.Size, TreeNodeHeaderFlags.PageRef, out pos))
                {
                    throw new InvalidOperationException($"balance page:{parent.Page.Header.PageNumber} failed!");
                }
            }

            parent.Page[index + 1].PageNumber = newEntry.Page.Header.PageNumber;
            parent.Page.InsertPageRefNode(index, pos, key, curEntry.Page.Header.PageNumber);
        }

        private TreeCursor BuildCursorForKey(TreeNodeDataSlice key, ILowLevelTransaction tx)
        {
            var entry = new TreePageEntry(_root, 0, null);

            while (entry.Page != null &&
                   entry.Page.SearchPageIfBranch(key, tx, out var page))
            {
                entry = new TreePageEntry(page, entry.Page.LastMatchIndex, entry);
            }

            return new TreeCursor(new Memory<byte>(key.Data.ToArray()), tx, entry);
        }

        private int SearchKeyForModify(TreePageEntry entry, TreeNodeDataSlice key)
        {
            entry.Page.Search(key);

            if (entry.Page.LastMatch < 0)
            {
                entry.Page.LastMatchIndex++;
            }

            return entry.Page.LastMatch;
        }

        private bool ModifyEntry(TreePageEntry entry, ILowLevelTransaction tx)
        {
            var lastMatch = entry.Page.LastMatch;
            var lastMatchIndex = entry.Page.LastMatchIndex;

            var newPage = tx.ModifyPage(entry.Page);
            if (newPage == null)
            {
                return false;
            }

            entry.Page = newPage;
            entry.Page.LastMatch = lastMatch;
            entry.Page.LastMatchIndex = lastMatchIndex;
            return true;
        }

        private unsafe TreePageEntry AllocateEntry(ILowLevelTransaction tx, TreeNodeFlags nodeFlags, TreePageEntry parent, int index)
        {
            var identity = tx.AllocatePage(0);
            var page = tx.ModifyPage(identity);
            var treePage = page.AsTreePage();

            ref var header = ref treePage.Header;

            header.ItemCount = 0;
            header.Flags = PageHeaderFlags.Tree;
            header.NodeFlags = nodeFlags;
            header.PagerId = identity.PagerId;
            header.PageNumber = identity.PageNumber;
            header.UsedLength = Constants.PageHeaderSize;
            header.Low = Constants.PageHeaderSize;
            header.Upper = Constants.PageSize;

            return new TreePageEntry(treePage, index, parent);
        }
    }
}
