using System;
using System.Linq;

namespace Vicuna.Storage.Trees
{
    public partial class TreeNodePage
    {
        public void InsertKey(ByteString key, StoragePosition value, out ByteString splitKey, out TreeNodePage splitPage)
        {
            if (IsLeaf)
            {
                InsertLeafNodeKey(key, value, out splitKey, out splitPage);
                return;
            }

            splitKey = null;
            splitPage = null;

            var insertIndex = SearchNodeKey(key);
            var childNodePage = StorageManager.LoadPage(NodeValues[insertIndex]);
            if (childNodePage == null)
            {
                throw new NullReferenceException(nameof(childNodePage));
            }

            childNodePage.InsertKey(key, value, out var childSplitKey, out var childSplitPage);

            if (childSplitPage == null)
            {
                UpdateMaxNodeKey(childNodePage);
            }
            else
            {
                NodeKeys.Insert(insertIndex + 1, childSplitKey);
                NodeValues.Insert(insertIndex + 1, new StoragePosition() { PageNumber = (uint)childSplitPage.PageId });
                Pages.Add(childSplitPage);
                UpdateMaxNodeKey(childSplitPage);
            }

            if (NodeKeys.Count > MaxCapacity)
            {
                SplitNodePage(ref splitKey, ref splitPage);
            }
        }

        public void InsertLeafNodeKey(ByteString key, StoragePosition value, out ByteString splitKey, out TreeNodePage splitPage)
        {
            splitKey = null;
            splitPage = null;

            // 查早插入健的位置
            var index = SearchNodeKey(key);
            if (index == -1)
            {
                NodeKeys.Add(key);
                NodeValues.Add(value);
                Capacity = (ushort)NodeKeys.Count;
            }
            else if (NodeKeys[index].Compare(key) == 0)
            {
                //重复Key,怎么处理?
                Capacity = (ushort)NodeKeys.Count;
            }
            else if (NodeKeys[index].Compare(key) > 0)
            {
                NodeKeys.Insert(index, key);
                NodeValues.Insert(index, value);
                Capacity = (ushort)NodeKeys.Count;
            }
            else
            {
                throw new InvalidOperationException();
            }

            if (NodeKeys.Count > MaxCapacity)
            {
                SplitNodePage(ref splitKey, ref splitPage);
            }
        }

        /// <summary>
        /// 从中间开始分割
        /// 记录分割节点的第一个键
        //  将指定分割点右侧的数据拷贝至新的节点，新节点为当前节点的右兄弟节点
        /// </summary>
        protected void SplitNodePage(ref ByteString splitKey, ref TreeNodePage splitPage)
        {
            var startIndex = NodeKeys.Count / 2;

            splitKey = NodeKeys[startIndex];
            splitPage = StorageManager.CreatePage();
            splitPage.NodeKeys = NodeKeys.Skip(startIndex).ToList();
            splitPage.NodeValues = NodeValues.Skip(startIndex).ToList();
            splitPage.Capacity = (ushort)splitPage.NodeKeys.Count;
            splitPage.NodeType = NodeType;

            NodeKeys = NodeKeys.Take(startIndex).ToList();
            NodeValues = NodeValues.Take(startIndex).ToList();
            Capacity = (ushort)NodeKeys.Count;
        }

        protected void UpdateMaxNodeKey(TreeNodePage childNodePage)
        {
            if (childNodePage.NodeKeys.Last().Compare(NodeKeys.Last()) > 0)
            {
                NodeKeys[NodeKeys.Count - 1] = childNodePage.NodeKeys.Last();
            }
        }
    }
}
