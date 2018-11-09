using System;

namespace Vicuna.Storage.Trees
{
    public class TreePage
    {
        public TreeNode<TKey> GetNode<TKey>()
            where TKey : IComparable
        {
            return null;
        }

        public TreeNode<TKey> AllocateNode<TKey>()
            where TKey : IComparable
        {
            return null;
        }

        public void RemoveNode()
        {

        }
    }
}
