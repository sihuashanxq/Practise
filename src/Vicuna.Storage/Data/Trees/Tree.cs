using System;
using System.Collections.Generic;
using System.Text;

namespace Vicuna.Storage.Data.Trees
{
    public class Tree
    {
        private TreePage _head;

        private bool _isMulpti;

        public long Get(ByteString key)
        {
            var treePath = GetTreePathWithKey(key);
            if (treePath != null)
            {
                var treePage = treePath.Peek();
                if (treePage == null)
                {
                    throw new NullReferenceException(nameof(treePage));
                }

                return treePage.Search(key, out var _, out var value) ? value : long.MinValue;
            }

            return long.MinValue;
        }

        public void Set(ByteString key, long value)
        {
            var treePath = GetTreePathWithKey(key);
            var treePage = treePath.Peek();
            if (treePage == null)
            {
                throw new NullReferenceException(nameof(treePage));
            }


            if (treePage.Search(key, out var index))
            {
                if (!_isMulpti)
                {
                    throw new InvalidCastException($"mulpti key {key.ToString()}");
                }

                return;
            }

            treePage.Insert(key, value, index);
            SplitTreePage(treePage, treePath);
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
            var midKey = treePage.GetKey(index);
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

        public void SplitTreePage2(TreePage parentPage, long pageNumber, long nextPageNumber, ByteString midKey)
        {
            if (parentPage.Header.ItemCount == 0)
            {
                parentPage.Insert(midKey, new ByteString(Encoding.UTF8.GetBytes(pageNumber.ToString()), 8), 0);
                parentPage.Insert(new ByteString(8), new ByteString(Encoding.UTF8.GetBytes(nextPageNumber.ToString()), 8), 1);
                return;
            }

            //1.5   2.5     0
            //1     2       4
            //1.5     2.5      3.5        0
            //1       2         3         4
            var found = parentPage.Search(midKey, out var index);
            if (index < parentPage.Header.ItemCount - 1)
            {
                parentPage.Insert(midKey, new ByteString(Encoding.UTF8.GetBytes(nextPageNumber.ToString()), 8), index);
            }
            else
            {
                parentPage.SetPageRef(midKey, parentPage.Header.ItemCount - 1);
                parentPage.Insert(new ByteString(8), new ByteString(Encoding.UTF8.GetBytes(nextPageNumber.ToString()), 8), parentPage.Header.ItemCount);
            }
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

        private Stack<TreePage> GetTreePathWithKey(ByteString key)
        {
            var path = new Stack<TreePage>(new[] { _head });
            var treePage = _head;

            while (treePage.Header.NodeType != TreeNodeFlags.Leaf)
            {
                treePage.Search(key, out var index, out var value);
                treePage = GetTreePage(value.ToInt64());

                path.Push(treePage);
            }

            if (treePage == null)
            {
                throw new NullReferenceException(nameof(treePage));
            }

            if (treePage.Header.NodeType != TreeNodeFlags.Leaf)
            {
                throw new InvalidOperationException($"tree page:{treePage.Header.PageNumber} is not a leaf b-tree page!");
            }

            return path;
        }

        private TreePage GetTreePage(long pageNumber)
        {
            return null;
        }
    }
}
