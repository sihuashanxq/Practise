using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vicuna.Storage.Transactions;

namespace Vicuna.Storage.Data.Trees
{
    public class Tree
    {
        private bool _isMulpti;

        public TreePage _root;

        public StorageLevelTransaction _tx;

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
                if (!_tx.AllocateTreePage(out var newPage))
                {

                    throw null;
                }
                fixed (byte* p = newPage.Header.MetaKeys)
                {
                    *p = (byte)DataValueType.String;
                }

                cursor.Current = new TreePageEntry(0, newPage);
                cursor.Current.Page.Header.Flags = Pages.PageFlags.Data;
                cursor.Current.Page.Header.NodeFlags = TreeNodeFlags.Leaf;
                _root = cursor.Current.Page;
            }

            var currentPage = cursor.Current.Page;
            var flag = currentPage.Search(key, out var index);
            if (flag == 0)
            {
                if (!_isMulpti)
                {
                    throw new InvalidCastException($"mulpti key");
                }
            }

            if (flag < 0)
            {
                index++;
            }

            var size = key.Size;
            if (key.Size + value.Size <= MaxPageDataSize)
            {
                size += (ushort)value.Size;
            }

            if (currentPage.Header.PageNumber == 102987)
            {

            }

            var currentEntry = cursor.Modify(_tx);
            if (currentEntry.Page.Allocate(index, size, flags, out var position))
            {
                currentEntry.Page.InsertDataNode(index, position, key, value, 0);
                if (cursor.IsRootChanged)
                {
                    _root = cursor.Pages[0].Page;
                }
                return;
            }

            if (CopyEntriesResult.StartNodeMovedToNewPage == Split(cursor, key, index))
            {
                index = 0;
                currentEntry = cursor.Current;
            }

            if (currentEntry.Page.Allocate(index, size, flags, out position))
            {
                currentEntry.Page.InsertDataNode(index, position, key, value, 0);
                if (cursor.IsRootChanged)
                {
                    _root = cursor.Pages[0].Page;
                }
                return;
            }

            throw new Exception();
        }

        public unsafe CopyEntriesResult Split(TreePageCursor cursor, TreeNodeKey key, int index)
        {
            if (!_tx.AllocateTreePage(out var newPage))
            {
                throw new Exception("allocate new page faild! ");
            }

            fixed (byte* p = newPage.Header.MetaKeys)
            {
                *p = (byte)DataValueType.String;
            }

            if (newPage.Header.PageNumber == 102987)
            {

            }

            newPage.Header.Flags = cursor.Current.Page.Header.Flags;
            newPage.Header.NodeFlags = cursor.Current.Page.Header.NodeFlags;
            using (cursor.CreateScope())
            {
                var current = cursor.Current;
                var newEntry = new TreePageEntry(current.Index + 1, newPage);
                var currrentPage = current.Page;
                if (index == currrentPage.Header.ItemCount)
                {
                    cursor.Current = newEntry;
                    cursor.Pop();
                    Split(cursor, key, current, newEntry);
                    return CopyEntriesResult.StartNodeMovedToNewPage;
                }

                if (currrentPage.CopyRightSideEntriesToNewPage(index, newPage) == CopyEntriesResult.StartNodeMovedToNewPage)
                {
                    cursor.Current = newEntry;
                    cursor.Pop();
                    Split(cursor, key, current, newEntry);
                    return CopyEntriesResult.StartNodeMovedToNewPage;
                }

                cursor.Pop();
                Split(cursor, newEntry.Page.MinKey, current, newEntry);
                return CopyEntriesResult.Normal;
            }
        }

        public unsafe void Split(TreePageCursor cursor, TreeNodeKey key, TreePageEntry currentEntry, TreePageEntry newEntry)
        {
            var parent = cursor.Current;
            if (parent == null)
            {
                if (!_tx.AllocateTreePage(out var parentPage))
                {
                    throw new Exception("allocate new page faild! ");
                }

                fixed (byte* p = parentPage.Header.MetaKeys)
                {
                    *p = (byte)DataValueType.String;
                }

                parentPage.Header.Flags = Pages.PageFlags.None;
                parentPage.Header.NodeFlags = TreeNodeFlags.Branch;

                //root
                parent = cursor.Current = new TreePageEntry(0, parentPage);
            }
            else
            {
                parent = cursor.Modify(_tx);
            }

            AddParentNodePageRef(cursor, key, parent, currentEntry, newEntry);
        }

        public unsafe void AddParentNodePageRef(TreePageCursor cursor, TreeNodeKey key, TreePageEntry parentEntry, TreePageEntry currentEntry, TreePageEntry newEntry)
        {
            if (newEntry.Page.Header.PageNumber == 103167)
            {
                Console.WriteLine(parentEntry.Page.Header.Upper);
            }

            if (parentEntry.Page.Header.ItemCount == 0)
            {
                newEntry.Index = 1;
                currentEntry.Index = 0;

                if (!parentEntry.Page.Allocate(currentEntry.Index, (ushort)(key.Size + sizeof(long)), TreeNodeHeaderFlags.PageRef, out var currentEntryPosition))
                {
                    throw new Exception("");
                }

                if (!parentEntry.Page.Allocate(newEntry.Index, sizeof(long), TreeNodeHeaderFlags.PageRef, out var newEntryPosition))
                {
                    throw new Exception("");
                }

                parentEntry.Page.InsertPageRefNode(0, currentEntryPosition, key, currentEntry.Page.Header.PageNumber);
                parentEntry.Page.InsertPageRefNode(1, newEntryPosition, new TreeNodeKey(), newEntry.Page.Header.PageNumber);
                return;
            }

            var index = currentEntry.Index;

            if (!parentEntry.Page.Allocate(index, (ushort)(key.Size + sizeof(long)), TreeNodeHeaderFlags.PageRef, out var position))
            {
                var halfEntryCount = parentEntry.Page.Header.ItemCount / 2;
                var halfEntryKey = parentEntry.Page.GetNodeKey(halfEntryCount);
                if (CopyEntriesResult.StartNodeMovedToNewPage == Split(cursor, halfEntryKey, halfEntryCount))
                {
                    if (halfEntryCount < newEntry.Index)
                    {
                        newEntry.Index = halfEntryCount - newEntry.Index - 1;
                    }

                    parentEntry = cursor.Current;
                }
                else
                {
                    if (halfEntryCount < newEntry.Index)
                    {
                        newEntry.Index = halfEntryCount - newEntry.Index;
                    }
                }

                if (!parentEntry.Page.Allocate(newEntry.Index, (ushort)(key.Size + sizeof(long)), TreeNodeHeaderFlags.PageRef, out position))
                {
                    throw new Exception("");
                }
            }

            if (newEntry.Index == parentEntry.Page.Header.ItemCount - 2)
            {

            }

            if (newEntry.Index <= parentEntry.Page.Header.ItemCount - 1)
            {
                var offset = parentEntry.Page.GetNodeOffset(index + 1);
                ref var node = ref parentEntry.Page.GetNodeHeader(offset);

                node.PageNumber = newEntry.Page.Header.PageNumber;

                parentEntry.Page.InsertPageRefNode(index, position, key, currentEntry.Page.Header.PageNumber);
            }
            else
            {
                var offset = parentEntry.Page.GetNodeOffset(parentEntry.Page.Header.ItemCount - 1);
                ref var node = ref parentEntry.Page.GetNodeHeader(offset);

                parentEntry.Page.InsertPageRefNode(newEntry.Index, position, key, node.PageNumber);

                node.PageNumber = newEntry.Page.Header.PageNumber;

                newEntry.Index++;
            }

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

            if (page.Header.NodeFlags != TreeNodeFlags.Leaf)
            {
                throw new InvalidOperationException($"tree page:{page.Header.PageNumber} is not a leaf b-tree page!");
            }

            return cursor;
        }

        private unsafe TreePage GetPage(long pageNumber)
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
    }

    public class TreePageCursor
    {
        public int Index;

        public List<TreePageEntry> Pages;

        public bool IsRootChanged => Pages[0] != null;

        public TreePageEntry Parent
        {
            get => Pages.Count > 1 ? Pages[Pages.Count - 2] : null;
        }

        public TreePageEntry Current
        {
            get
            {
                if (Index > Pages.Count - 1 || Index < 0)
                {
                    throw new IndexOutOfRangeException();
                }

                return Pages[Index];
            }
            internal set
            {
                if (Index > Pages.Count - 1 || Index < 0)
                {
                    throw new IndexOutOfRangeException();
                }

                Pages[Index] = value;
            }
        }

        internal TreePageCursor()
        {
            Index = -1;
            Pages = new List<TreePageEntry>();
        }

        internal TreePageCursor(IEnumerable<TreePageEntry> pages) : this()
        {
            foreach (var item in pages)
            {
                Pages.Add(item);
            }
        }

        public TreePageEntry Pop()
        {
            if (Index > Pages.Count)
            {
                return null;
            }

            var page = Pages[Index];

            Index--;

            return page;
        }

        public void Push(TreePageEntry newPage)
        {
            if (Index >= Pages.Count - 1)
            {
                Index++;
                Pages.Add(newPage);
            }
            else
            {
                Index++;
                Pages.Insert(Index, newPage);
            }
        }

        public TreePageEntry Modify(StorageLevelTransaction tx)
        {
            var page = tx.GetPageToModify2(Current.Page.Header.PageNumber);

            return Current = new TreePageEntry(Current.Index, new TreePage(page));
        }

        public void Reset()
        {
            Index = Pages.Count - 1;
        }

        public TreePageCursorScope CreateScope()
        {
            return new TreePageCursorScope(this);
        }
    }

    public class TreePageCursorScope : IDisposable
    {
        private int _index;

        public TreePageCursor Cursor;

        public TreePageCursorScope(TreePageCursor cursor)
        {
            _index = cursor.Index;
            Cursor = cursor;
        }

        public void Dispose()
        {
            Cursor.Index = _index;
        }
    }

    public class TreePageEntry
    {
        public int Index { get; set; }

        public TreePage Page { get; set; }

        public TreePageEntry(int index, TreePage page)
        {
            Index = index;
            Page = page;
        }
    }
}
