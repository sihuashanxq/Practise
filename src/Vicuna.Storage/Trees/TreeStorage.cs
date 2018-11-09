using System;
using System.Collections.Generic;
using System.IO;

namespace Vicuna.Storage.Trees
{
    public class TreeStorage<TKey>
        where TKey : IComparable
    {
        public int NodeCapacity => (8192 * 2 - 30 - 2) / 14;

        public static int PageIndex = 2;

        public Dictionary<long, TreeNode<TKey>> _pages = new Dictionary<long, TreeNode<TKey>>();

        public Stream Stream = new MemoryStream();

        public long RootId { get; set; }

        public TreeNode<TKey> Create(bool isLeaf)
        {
            var p = new TreeNode<TKey>(PageIndex++) { IsLeaf = isLeaf };
            _pages[p.NodeId] = p;
            return p;
        }

        public TreeNode<TKey> CreateRoot()
        {
            var p = new TreeNode<TKey>(PageIndex++) { IsLeaf = false };
            _pages[p.NodeId] = p;
            SetRoot(p.NodeId);
            return p;
        }

        public TreeNode<TKey> GetNode(long nodeIndex)
        {
            if (_pages.TryGetValue(nodeIndex, out var page))
            {
                return page;
            }

            var node = new TreeNode<TKey>(nodeIndex);
            var buffer = GetPage(nodeIndex);

            node.Load(buffer);

            if (node.Keys.Count == 0)
            {
                node.IsLeaf = true;
            }

            if (node.NextNodeId == 0)
            {
                node.NextNodeId = -1;
            }

            if (node.PrevNodeId == 0)
            {
                node.PrevNodeId = -1;
            }

            if (node.ParentNodeId == 0)
            {
                node.ParentNodeId = -1;
            }

            _pages[nodeIndex] = node;

            return node;
        }

        public TreeNode<TKey> GetRoot()
        {
            if (RootId == 0)
            {
                var page = GetPage(0);
                var rootindex = BitConverter.ToInt64(page);
                var p = GetNode(rootindex == 0 ? 1 : rootindex);

                if (rootindex == 0)
                {
                    SetRoot(1);
                }

                return p;
            }

            return GetNode(RootId);
        }

        public void SetRoot(long nodeIndex)
        {
            RootId = nodeIndex;
        }

        public byte[] GetPage(long pageIndex)
        {
            var start = pageIndex * 8192 * 2;
            var end = pageIndex * 8192 * 2 + 8192 * 2;

            if (Stream.Length < end)
            {
                Stream.SetLength(end);
            }

            Stream.Seek(start, SeekOrigin.Begin);

            var buffer = new byte[8192 * 2];

            Stream.Read(buffer, 0, 8192 * 2);

            return buffer;
        }
    }
}
