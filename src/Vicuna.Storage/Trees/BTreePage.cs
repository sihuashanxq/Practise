using System;

namespace Vicuna.Storage.Trees
{
    public class BTreePage
    {
        public BTreeNode<TKey> GetNode<TKey>()
            where TKey : IComparable
        {
            return null;
        }

        public BTreeNode<TKey> AllocateNode<TKey>()
            where TKey : IComparable
        {
            return null;
        }

        public void RemoveNode()
        {

        }
    }
}
