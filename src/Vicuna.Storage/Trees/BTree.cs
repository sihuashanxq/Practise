using System;
using System.Collections.Generic;
using System.Linq;

namespace Vicuna.Storage.Trees
{
    public class BTree<TKey> where TKey : IComparable
    {
        public BTreeStorage<TKey> _storage;

        private BTreeNode<TKey> GetRoot()
        {
            return _storage.GetRoot();
        }

        private BTreeNode<TKey> GetNode(long index)
        {
            return _storage.GetNode(index);
        }

        private BTreeNode<TKey> CreateNode(bool isLeaf)
        {
            return _storage.Create(isLeaf);
        }

        private BTreeNode<TKey> CreateRootNode()
        {
            return _storage.CreateRoot();
        }

        public long Get(TKey key)
        {
            var node = GetNodeForKey(key);
            if (node == null)
            {
                return -1;
            }

            if (SearchKey(node, key, out var index))
            {
                return node.Values[index];
            }

            return -1;
        }

        public void Set(TKey key, long value)
        {
            var node = GetNodeForKey(key);
            if (node == null)
            {
                throw new NullReferenceException(nameof(node));
            }

            Set(node, key, value);
        }

        public void Set(BTreeNode<TKey> node, TKey key, long value)
        {
            if (SearchKey(node, key, out var index))
            {
                return;
            }

            node.Keys.Insert(index, key);
            node.Values.Insert(index, value);
            Split(node);
        }

        public void Remove(TKey key)
        {
            var node = GetNodeForKey(key);
            if (node == null)
            {
                return;
            }

            if (SearchKey(node, key, out var index))
            {
                node.Keys.RemoveAt(index);
                node.Values.RemoveAt(index);
                Merge(node);
            }
        }

        private void Split(BTreeNode<TKey> node)
        {
            if (!IsFull(node))
            {
                return;
            }

            var nNextNode = CreateNode(node.IsLeaf);
            var parentNode = node.IsRoot ? CreateRootNode() : GetNode(node.ParentNodeId);
            if (parentNode == null)
            {
                throw new NullReferenceException(nameof(parentNode));
            }

            var oNextNode = node.HasNext ? GetNode(node.NextNodeId) : null;
            if (oNextNode != null)
            {
                oNextNode.IsDirty = true;
                oNextNode.PrevNodeId = nNextNode.NodeId;
            }

            nNextNode.IsDirty = true;
            nNextNode.PrevNodeId = node.NodeId;
            nNextNode.NextNodeId = node.NextNodeId;
            nNextNode.ParentNodeId = parentNode.NodeId;

            node.NextNodeId = nNextNode.NodeId;
            node.ParentNodeId = parentNode.NodeId;

            Split(parentNode, node, nNextNode);
        }

        private void Split(BTreeNode<TKey> parentNode, BTreeNode<TKey> node, BTreeNode<TKey> nNextNode)
        {
            var spliteIndex = _storage.NodeCapacity / 2;
            var spliteKey = node.Keys[spliteIndex];

            if (node.IsLeaf)
            {
                nNextNode.Keys = node.Keys.Skip(spliteIndex).ToList();
                nNextNode.Values = node.Values.Skip(spliteIndex).ToList();

                node.Keys = node.Keys.Take(spliteIndex).ToList();
                node.Values = node.Values.Take(spliteIndex).ToList();
            }
            else
            {
                nNextNode.Keys = node.Keys.Skip(spliteIndex + 1).ToList();
                nNextNode.Values = node.Values.Skip(spliteIndex + 1).ToList();

                node.Keys = node.Keys.Take(spliteIndex).ToList();
                node.Values = node.Values.Take(spliteIndex + 1).ToList();
            }

            UpdateChildrenParentReference(nNextNode);
            Split(parentNode, node, nNextNode, spliteKey);
        }

        private void Split(BTreeNode<TKey> parentNode, BTreeNode<TKey> node, BTreeNode<TKey> nNextNode, TKey splitedKey)
        {
            SearchKey(parentNode, splitedKey, out var keyInParentIndex);

            if (parentNode.Values.Count > keyInParentIndex)
            {
                parentNode.Values.RemoveAt(keyInParentIndex);
            }

            parentNode.Keys.Insert(keyInParentIndex, splitedKey);
            parentNode.Values.Insert(keyInParentIndex, node.NodeId);
            parentNode.Values.Insert(keyInParentIndex + 1, nNextNode.NodeId);
            Split(parentNode);
        }

        private void Merge(BTreeNode<TKey> node, BTreeNode<TKey> sibling)
        {
            var parent = GetNode(node.ParentNodeId);

            parent.IsDirty = true;

            node.IsDirty = true;
            node.NextNodeId = sibling.NextNodeId;

            sibling.IsDeleted = true;

            if (sibling.HasNext)
            {
                var next = GetNode(sibling.NextNodeId);
                next.IsDirty = true;
                next.PrevNodeId = node.NodeId;
            }

            Merge(parent, node, sibling);
        }

        private void Merge(BTreeNode<TKey> parent, BTreeNode<TKey> node, BTreeNode<TKey> sibling)
        {
            if (node.IsLeaf)
            {
                node.Keys.AddRange(sibling.Keys);
                node.Values.AddRange(sibling.Values);
            }
            else
            {
                node.Keys.AddRange(sibling.Keys);
                node.Values.AddRange(sibling.Values);
                UpdateNodeParentReference(sibling.Values, node.NodeId);
            }

            if (parent.IsRoot && parent.Keys.Count == 1)
            {
                node.ParentNodeId = -1;
                parent.IsDeleted = true;
                _storage.SetRoot(node.NodeId);
                return;
            }

            SearchKey(parent, node.MinKey, out var index);

            parent.Keys.RemoveAt(index);
            parent.Values.RemoveAt(index + 1);

            if (parent.IsRoot && parent.Keys.Count == 1)
            {
                node.ParentNodeId = -1;
                parent.IsDeleted = true;
                _storage.SetRoot(node.NodeId);
                return;
            }

            Merge(parent);
        }

        private void Merge(BTreeNode<TKey> node)
        {
            if (IsValid(node))
            {
                return;
            }

            if (node.HasPrev)
            {
                var prev = GetNode(node.PrevNodeId);
                if (prev.ParentNodeId == node.ParentNodeId)
                {
                    Merge(prev, node);
                    Split(prev);
                    return;
                }
            }

            if (node.HasNext)
            {
                var next = GetNode(node.NextNodeId);
                if (next.ParentNodeId == node.ParentNodeId)
                {
                    Merge(node, next);
                    Split(node);
                }
            }
        }

        private void UpdateChildrenParentReference(BTreeNode<TKey> node)
        {
            if (node.IsLeaf)
            {
                return;
            }

            UpdateNodeParentReference(node.Values, node.NodeId);
        }

        private void UpdateNodeParentReference(IEnumerable<long> nodes, long parentId)
        {
            foreach (var item in nodes)
            {
                var node = GetNode(item);
                if (node == null)
                {
                    throw new NullReferenceException(nameof(node));
                }

                node.IsDirty = true;
                node.ParentNodeId = parentId;
            }
        }

        private BTreeNode<TKey> GetNodeForKey(TKey key)
        {
            var node = GetRoot();
            while (true)
            {
                if (node == null || node.IsLeaf)
                {
                    break;
                }

                SearchKey(node, key, out var index);
                node = GetNode(node.Values[index]);
            }

            if (node == null)
            {
                throw new NullReferenceException(nameof(node));
            }

            if (!node.IsLeaf)
            {
                throw new InvalidOperationException($"node:{node.NodeId} is not a leaf node!");
            }

            return node;
        }

        private bool IsFull(BTreeNode<TKey> node)
        {
            return node.Keys.Count >= _storage.NodeCapacity;
        }

        private bool IsValid(BTreeNode<TKey> node)
        {
            return node.Keys.Count > _storage.NodeCapacity / 2;
        }

        private static bool SearchKey(BTreeNode<TKey> node, TKey key, out int index)
        {
            if (!node.Keys.Any())
            {
                index = 0;
                return false;
            }

            var first = 0;
            var last = node.Keys.Count - 1;

            //如果key小于节点的第一个key,直接返回
            if (node.MinKey.CompareTo(key) == 1)
            {
                index = 0;
                return false;
            }

            //如果key大于节点的最后一个key,直接返回
            if (node.MaxKey.CompareTo(key) == -1)
            {
                index = node.Keys.Count;
                return false;
            }

            //二分查找,如果key>mid,向后查找,否则向前查找
            while (first < last)
            {
                var mid = first + (last - first) / 2;
                var midKey = node.Keys[mid];
                var flag = midKey.CompareTo(key);
                if (flag == 0)
                {
                    index = node.IsLeaf ? mid : mid + 1;
                    return true;
                }

                if (flag == 1)
                {
                    last = mid;
                    continue;
                }

                first = mid + 1;
            }

            switch (node.Keys[last].CompareTo(key))
            {
                case 0:
                    index = node.IsLeaf ? last : last + 1;
                    return true;
                default:
                    //must be >
                    index = last;
                    return false;
            }
        }
    }
}
