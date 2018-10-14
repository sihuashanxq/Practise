using System;

namespace Vicuna.Storage.Trees
{
    public partial class TreeNodePage
    {
        public int SearchNodeKey(ByteString searchKey)
        {
            for (var i = 0; i < NodeKeys.Count; i++)
            {
                var key = NodeKeys[i];
                if (key.Compare(searchKey) >= 0)
                {
                    return i;
                }
            }

            return -1;
        }

        public int SearchNodeKey(ByteString searchKey, out TreeNodePage node)
        {
            if (IsLeaf)
            {
                node = this;
                return SearchNodeKey(searchKey);
            }

            var index = SearchNodeKey(searchKey);
            if (index > NodeValues.Count - 1)
            {
                throw new IndexOutOfRangeException(nameof(index));
            }

            var position = NodeValues[index];
            var nodePage = StorageManager.LoadPage(position);
            if (nodePage == null)
            {
                throw new NullReferenceException(nameof(nodePage));
            }

            return nodePage.SearchNodeKey(searchKey, out node);
        }
    }
}
