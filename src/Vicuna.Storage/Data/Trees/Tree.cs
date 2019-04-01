using System;
using Vicuna.Storage.Paging;
using Vicuna.Storage.Transactions;

namespace Vicuna.Storage.Data.Trees
{
    public class Tree
    {
        private bool _isMulpti;

        public TreePage _root;

        public IStorageTransaction _tx;

        public const ushort MaxPageDataSize = (Constants.PageSize - Constants.PageHeaderSize) / 2 - TreeNodeHeader.SizeOf - TreeNodeTransactionHeader.SizeOf;

        public TreeNodeValue Get(TreeNodeKey key)
        {
            var cursor = SearchForKey(key);
            if (cursor.Current == null)
            {
                return new TreeNodeValue();
            }

            if (cursor.Current.Page.Search(key, out var index) == 0)
            {
                return cursor.Current.Page.GetNodeValue(index);
            }

            return new TreeNodeValue();
        }

        public unsafe void Insert(TreeNodeKey key, TreeNodeValue value, TreeNodeHeaderFlags flags)
        {
            var cursor = SearchForKey(key);
            if (cursor.Current == null)
            {
                cursor.Current = new TreePageEntry(0, AllocatePage(TreeNodeFlags.Leaf, Pages.PageFlags.Data));
            }

            var flag = cursor.Current.Page.Search(key, out var index);
            if (flag < 0)
            {
                index++;
            }

            if (flag == 0)
            {
                if (!_isMulpti)
                {
                    throw new InvalidCastException($"mulpti key");
                }
            }

            var size = key.Size;
            if (size + value.Size <= MaxPageDataSize)
            {
                size += (ushort)value.Size;
            }

            var curPageEntry = cursor.Modify(_tx);
            if (!curPageEntry.Page.Allocate(index, size, flags, out var position))
            {
                var balanceIndex = Balance(cursor, key, index);
                if (balanceIndex <= index)
                {
                    index = index - balanceIndex;
                }

                if (!cursor.Current.Page.Allocate(index, size, flags, out position))
                {
                    throw new InvalidOperationException("allocate node space faild!");
                }

                curPageEntry = cursor.Current;
            }

            _root = cursor.Root.Page;

            curPageEntry.Page.InsertDataNode(index, position, key, value, 0);
        }

        public unsafe int Balance(TreePageCursor cursor, TreeNodeKey key, int index)
        {
            using (cursor.CreateScope())
            {
                var curPageEntry = cursor.Current;
                var newPage = AllocatePage(curPageEntry.Page.Header.NodeFlags, curPageEntry.Page.Header.Flags);
                var newPageEntry = new TreePageEntry(curPageEntry.Index + 1, newPage);
                var offset = curPageEntry.Page.CopyNodeEntriesToNewPage(index, newPage);
                if (offset <= index)
                {
                    index = index - offset;
                    key = index > 0 ? newPage.MinKey : key;
                    cursor.Current = newPageEntry;
                }
                else
                {
                    key = newPage.MinKey;
                }

                cursor.Pop();
                Balance(cursor, key, curPageEntry, newPageEntry);
                return offset;
            }
        }

        public unsafe void Balance(TreePageCursor cursor, TreeNodeKey key, TreePageEntry curPageEntry, TreePageEntry newPageEntry)
        {
            var parent = cursor.Current;
            if (parent == null)
            {
                //root
                parent = cursor.Current = new TreePageEntry(0, AllocatePage(TreeNodeFlags.Branch, Pages.PageFlags.Data));
            }
            else
            {
                parent = cursor.Modify(_tx);
            }

            Balance(cursor, key, parent, curPageEntry, newPageEntry);
        }

        public unsafe void Balance(TreePageCursor cursor, TreeNodeKey key, TreePageEntry parentEntry, TreePageEntry currentEntry, TreePageEntry newEntry)
        {
            var index = currentEntry.Index;

            if (parentEntry.Page.Header.ItemCount == 0)
            {
                newEntry.Index = 1;
                currentEntry.Index = 0;

                parentEntry.Page.Allocate(0, key.Size, TreeNodeHeaderFlags.PageRef, out var currentEntryPosition);
                parentEntry.Page.Allocate(1, 0, TreeNodeHeaderFlags.PageRef, out var newEntryPosition);

                parentEntry.Page.InsertPageRefNode(0, currentEntryPosition, key, currentEntry.Page.Header.PageNumber);
                parentEntry.Page.InsertPageRefNode(1, newEntryPosition, new TreeNodeKey(), newEntry.Page.Header.PageNumber);
                return;
            }

            if (!parentEntry.Page.Allocate(index, key.Size, TreeNodeHeaderFlags.PageRef, out var position))
            {
                var halfEntryIndex = parentEntry.Page.Header.ItemCount / 2;
                var halfEntryKey = parentEntry.Page.GetNodeKey(halfEntryIndex - 1);
                var balanceIndex = Balance(cursor, halfEntryKey, halfEntryIndex);
                if (balanceIndex <= halfEntryIndex && balanceIndex <= currentEntry.Index)
                {
                    index = currentEntry.Index - balanceIndex;
                    newEntry.Index = index + 1;
                    parentEntry = cursor.Current;
                }
                else if (balanceIndex > halfEntryIndex && balanceIndex < currentEntry.Index)
                {
                    index = currentEntry.Index - balanceIndex - 1;
                    newEntry.Index = index + 1;
                    parentEntry = cursor.Current;
                }

                if (!parentEntry.Page.Allocate(index, key.Size, TreeNodeHeaderFlags.PageRef, out position))
                {

                }
            }

            parentEntry.Page[index + 1].PageNumber = newEntry.Page.Header.PageNumber;
            parentEntry.Page.InsertPageRefNode(index, position, key, currentEntry.Page.Header.PageNumber);
        }

        private TreePageCursor SearchForKey(TreeNodeKey key)
        {
            var cursor = new TreePageCursor();
            var page = _root;
            if (page == null)
            {
                cursor.Push(null);
                return cursor;
            }

            cursor.Push(null);
            cursor.Push(new TreePageEntry(0, _root));

            while (!page.IsLeaf)
            {
                if (page.Search(key, out var index, out var _, out var node) == 0 && node.HasValue)
                {
                    page = GetPage(node.Value.PageNumber);
                    cursor.Push(new TreePageEntry(index, page));
                }
            }

            return cursor;
        }

        public unsafe TreePage GetPage(long pageNumber)
        {
            var page = _tx.GetPage(pageNumber);
            if (page == null)
            {
                return null;
            }


            var p2 = new TreePage(page.Buffer);

            p2.Header.PageNumber = pageNumber;

            fixed (byte* p = p2.Header.MetaKeys)
            {
                *p = (byte)DataValueType.String;
            }

            return p2;
        }

        public unsafe TreePage AllocatePage(TreeNodeFlags nodeFlags, PageFlags pageFlags)
        {
            _tx.AllocateTreePage(out var newPage);

            newPage.Header.Flags = pageFlags;
            newPage.Header.NodeFlags = nodeFlags;

            fixed (byte* p = newPage.Header.MetaKeys)
            {
                *p = (byte)DataValueType.String;
            }

            return newPage;
        }
    }
}
