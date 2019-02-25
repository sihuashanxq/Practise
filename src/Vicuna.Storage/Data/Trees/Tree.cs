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

                cursor.Current = new TreePathEntry(0, newPage);
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

            var currentEntry = cursor.Modify(_tx);
            if (currentEntry.Page.Allocate(index, size, flags, out var position))
            {
                currentEntry.Page.InsertDataNode(index, position, key, value, 0);
                return;
            }

            if (CopyEntriesResult.StartNodeMovedToNewPage == Split(cursor, index))
            {
                index = 0;
                currentEntry = cursor.Current;
            }

            if (currentEntry.Page.Allocate(index, size, flags, out position))
            {
                currentEntry.Page.InsertDataNode(index, position, key, value, 0);
                return;
            }

            throw new Exception();
        }

        public unsafe CopyEntriesResult Split(TreePathCursor cursor, int index)
        {
            if (!_tx.AllocateTreePage(out var newPage))
            {
                throw new Exception("allocate new page faild! ");
            }

            fixed (byte* p = newPage.Header.MetaKeys)
            {
                *p = (byte)DataValueType.String;
            }


            using (cursor.CreateScope())
            {
                var current = cursor.Current;
                var newEntry = new TreePathEntry(current.Index + 1, newPage);
                var currrentPage = current.Page;
                if (currrentPage.CopyRightSideEntriesToNewPage(index, null) == CopyEntriesResult.StartNodeMovedToNewPage)
                {
                    cursor.Current = newEntry;
                    cursor.Pop();
                    Split(cursor, newEntry);
                    return CopyEntriesResult.StartNodeMovedToNewPage;
                }

                cursor.Pop();
                Split(cursor, newEntry);
                return CopyEntriesResult.Normal;
            }
        }

        public unsafe void Split(TreePathCursor cursor, TreePathEntry newEntry)
        {
            var key = newEntry.Page.GetNodeKey(0);
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

                //root
                parent = cursor.Current = new TreePathEntry(0, parentPage);
            }
            else
            {
                parent = cursor.Modify(_tx);
            }

            if (!parent.Page.Allocate(newEntry.Index, (ushort)(key.Size + sizeof(long)), TreeNodeHeaderFlags.PageRef, out var position))
            {
                var halfEntryCount = parent.Page.Header.ItemCount / 2;

                if (CopyEntriesResult.StartNodeMovedToNewPage == Split(cursor, halfEntryCount))
                {
                    if (halfEntryCount < newEntry.Index)
                    {
                        newEntry.Index = halfEntryCount - newEntry.Index - 1;
                    }

                    parent = cursor.Current;
                }
                else
                {
                    if (halfEntryCount < newEntry.Index)
                    {
                        newEntry.Index = halfEntryCount - newEntry.Index;
                    }
                }
            }

            if (!parent.Page.Allocate(newEntry.Index, (ushort)(key.Size + sizeof(long)), TreeNodeHeaderFlags.PageRef, out position))
            {
                throw new Exception("");
            }

            if (newEntry.Index < parent.Page.Header.ItemCount - 1)
            {
                parent.Page.InsertPageRefNode(newEntry.Index + 1, position, key, newEntry.Page.Header.PageNumber);
            }
            else
            {
                var offset = parent.Page.GetNodeOffset(parent.Page.Header.ItemCount - 1);
                ref var node = ref parent.Page.GetNodeHeader(offset);

                parent.Page.InsertPageRefNode(newEntry.Index, position, key, node.PageNumber);

                node.PageNumber = newEntry.Page.Header.PageNumber;

                newEntry.Index++;
            }
        }

        private TreePathCursor SearchForKey(TreeNodeKey key)
        {
            var cursor = new TreePathCursor();
            var page = _root;
            if (page == null)
            {
                cursor.Push(null);
                return cursor;
            }

            cursor.Push(null);
            cursor.Push(new TreePathEntry(0, _root));

            return cursor;
            //while (!page.IsLeaf)
            //{
            //    if (page.Search(key, out var index, out var _, out var node) && node.HasValue)
            //    {
            //        page = GetPage(node.Value.PageNumber);
            //        cursor.Push(new TreePathEntry(index, page));
            //    }
            //}

            //if (page.Header.NodeFlags != TreeNodeFlags.Leaf)
            //{
            //    throw new InvalidOperationException($"tree page:{page.Header.PageNumber} is not a leaf b-tree page!");
            //}

            //return cursor;
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

    public class TreePathCursor
    {
        public int Index;

        public List<TreePathEntry> Pages;

        public bool IsRootChanged { get; set; }

        public TreePathEntry Parent
        {
            get => Pages.Count > 1 ? Pages[Pages.Count - 2] : null;
        }

        public TreePathEntry Current
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

        internal TreePathCursor()
        {
            Index = -1;
            Pages = new List<TreePathEntry>();
        }

        internal TreePathCursor(IEnumerable<TreePathEntry> pages) : this()
        {
            foreach (var item in pages)
            {
                Pages.Add(item);
            }
        }

        public TreePathEntry Pop()
        {
            if (Index > Pages.Count)
            {
                return null;
            }

            var page = Pages[Index];

            Index--;

            return page;
        }

        public void Push(TreePathEntry newPage)
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

        public TreePathEntry Modify(StorageLevelTransaction tx)
        {
            var page = tx.GetPageToModify2(Current.Page.Header.PageNumber);

            return Current = new TreePathEntry(Current.Index, new TreePage(page));
        }

        public void Reset()
        {
            Index = Pages.Count - 1;
        }

        public TreePathCursorScope CreateScope()
        {
            return new TreePathCursorScope(this);
        }
    }

    public class TreePathCursorScope : IDisposable
    {
        private int _index;

        public TreePathCursor Cursor;

        public TreePathCursorScope(TreePathCursor cursor)
        {
            _index = cursor.Index;
            Cursor = cursor;
        }

        public void Dispose()
        {
            Cursor.Index = _index;
        }
    }

    public class TreePathEntry
    {
        public int Index { get; set; }

        public TreePage Page { get; set; }

        public TreePathEntry(int index, TreePage page)
        {
            Index = index;
            Page = page;
        }
    }
}
