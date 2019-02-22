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

        private TreePage _root;

        private StorageLevelTransaction _tx;

        public const ushort MaxPageDataSize = (Constants.PageSize - Constants.PageHeaderSize) / 2 - TreeNodeHeader.SizeOf - TreeNodeTransactionHeader.SizeOf;

        public TreeNodeValue Get(TreeNodeKey key)
        {
            var cursor = SearchForKey(key);
            if (cursor != null)
            {
                //return cursor.Current.Search(key, out var _, out var value,out ) ? value : long.MinValue;
            }

            return new TreeNodeValue();
        }

        public void Insert(TreeNodeKey key, TreeNodeValue value, TreeNodeHeaderFlags flags)
        {

            var cursor = SearchForKey(key);
            if (cursor.Current.IsBranch)
            {
                throw new InvalidOperationException();
            }

            if (cursor.Current.Search(key, out var index) && !_isMulpti)
            {
                throw new InvalidCastException($"mulpti key");
            }

            var keySize = key.Size;
            var valueSize = value.Size;
            if (valueSize + keySize > MaxPageDataSize)
            {
                valueSize = 0;
            }

            if (!cursor.Current.Allocate(index, (ushort)(keySize + valueSize), flags, out var position))
            {
                if (!_tx.AllocatePage(out var newPage))
                {
                    throw new Exception(" allocate new page faild! ");
                }

                var isStartNodeMovedToNewPage = false;
                var currentPage = cursor.Current;

                currentPage.CopyRightSideEntriesToNewPage(index, null, out isStartNodeMovedToNewPage);
                //split page
            }

            cursor.Current.InsertDataNode(index, position, key, value, 0);
        }

        public void Split(TreePathCursor cursor, int index)
        {

        }

        public void SplitTreePage(TreePage treePage, Stack<TreePage> treePath)
        {
            if (treePage == treePath.Peek())
            {
                treePath.Pop();
            }

            var nextPage = CreateNewPage(treePage.Header.NodeType);
            var parentPage = treePath.Count == 0 ? CreateNewPage(TreeNodeFlags.Root) : treePath.Pop();
            if (parentPage == null)
            {
                throw new NullReferenceException(nameof(parentPage));
            }

            var oNextPage = treePage.Header.NextPageNumber != -1 ? GetTreePage(treePage.Header.NextPageNumber) : null;
            if (oNextPage != null)
            {
                oNextPage.Header.PrePageNumber = nextPage.Header.PageNumber;
            }

            nextPage.Header.PrePageNumber = treePage.Header.PageNumber;
            nextPage.Header.NextPageNumber = treePage.Header.NextPageNumber;
            treePage.Header.NextPageNumber = nextPage.Header.PageNumber;

            SplitTreePage(parentPage, nextPage, nextPage, treePath);
        }

        public void SplitTreePage(TreePage parentPage, TreePage treePage, TreePage nextPage, Stack<TreePage> treePath)
        {
            var index = treePage.Header.ItemCount / 2;
            //var midKey = treePage.GetKey(index);
            var movedPairs = treePage.Remove(index, treePage.Header.ItemCount - index);

            for (var i = treePage.IsBranch ? 1 : 0; i < movedPairs.Count; i++)
            {
                nextPage.Insert(movedPairs[i].Key, movedPairs[i].Value, i);
            }

            if (treePage.IsBranch)
            {
                treePage.Insert(new ByteString(treePage.Header.KeySize), movedPairs[0].Value, index);
            }

            SplitTreePage(parentPage, treePage, nextPage, midKey, treePath);
        }

        public void SplitTreePage(TreePage parentPage, TreePage treePage, TreePage nextPage, ByteString midKey, Stack<TreePage> treePath)
        {
            if (parentPage.Header.ItemCount == 0)
            {
                parentPage.Insert(midKey, new ByteString(BitConverter.GetBytes(treePage.Header.PageNumber)), 0);
                parentPage.Insert(new ByteString(treePage.Header.KeySize), new ByteString(BitConverter.GetBytes(nextPage.Header.PageNumber)), 1);
                return;
            }

            //1.5   2.5     0
            //1     2       4
            //1.5     2.5      3.5        0
            //1       2         3         4
            var found = parentPage.Search(midKey, out var index);
            if (index < parentPage.Header.ItemCount - 1)
            {
                parentPage.Insert(midKey, new ByteString(BitConverter.GetBytes(nextPage.Header.PageNumber)), index + 1);
            }
            else
            {
                parentPage.SetPageRef(midKey, parentPage.Header.ItemCount - 1);
                parentPage.Insert(new ByteString(treePage.Header.KeySize), new ByteString(BitConverter.GetBytes(nextPage.Header.PageNumber)), 1);
            }

            SplitTreePage(parentPage, treePath);
        }

        private TreePage CreateNewPage(TreeNodeFlags nodeType)
        {
            return null;
        }

        //public void Remove(TKey key)
        //{
        //    var node = GetNodeForKey(key);
        //    if (node == null)
        //    {
        //        return;
        //    }

        //    if (SearchKey(node, key, out var index))
        //    {
        //        node.Keys.RemoveAt(index);
        //        node.Values.RemoveAt(index);
        //        Merge(node);
        //    }
        //}

        private TreePathCursor SearchForKey(TreeNodeKey key)
        {
            var cursor = new TreePathCursor(new[] { _root });
            var page = _root;

            while (!page.IsLeaf)
            {
                if (page.Search(key, out var index, out var _, out var node) && node.HasValue)
                {
                    page = GetTreePage(node.Value.PageNumber);
                    cursor.Push(page);
                }
            }

            if (page.Header.NodeFlags != TreeNodeFlags.Leaf)
            {
                throw new InvalidOperationException($"tree page:{page.Header.PageNumber} is not a leaf b-tree page!");
            }

            return cursor;
        }

        private TreePage GetTreePage(long pageNumber)
        {
            return null;
        }
    }

    public class TreePathCursor
    {
        internal List<TreePage> Pages { get; }

        public int Count
        {
            get => Pages.Count;
        }

        public TreePage Parent
        {
            get => Count > 1 ? Pages[Count - 2] : null;
        }

        public TreePage Current
        {
            get => Pages.LastOrDefault();
        }

        internal TreePathCursor()
        {
            Pages = new List<TreePage>();
        }

        internal TreePathCursor(IEnumerable<TreePage> pages) : this()
        {
            foreach (var item in pages)
            {
                Pages.Add(item);
            }
        }

        public TreePage Pop()
        {
            if (Count == 0)
            {
                return null;
            }

            var page = Current;

            Pages.RemoveAt(Count - 1);

            return page;
        }

        public void Push(TreePage newPage)
        {
            Pages.Add(newPage);
        }

        public TreePathCursor CreateScope()
        {
            return new TreePathCursor(Pages);
        }
    }
}
