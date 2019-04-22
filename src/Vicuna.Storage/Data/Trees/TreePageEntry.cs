using Vicuna.Storage.Transactions;

namespace Vicuna.Storage.Data.Trees
{
    public class TreePageEntry
    {
        public int Index { get; set; }

        public TreePage Page { get; set; }

        public TreePageEntry Parent { get; set; }

        public int LastMatch
        {
            get => Page.LastMatch;
            set => Page.LastMatch = value;
        }

        public int LastMatchIndex
        {
            get => Page.LastMatchIndex;
            set => Page.LastMatchIndex = value;
        }

        public ref TreePageHeader Header => ref Page.Header;

        public TreeNodeDataSlice MinKey => Page.MinKey;

        public TreeNodeDataSlice MaxKey => Page.MaxKey;

        public TreePageEntry(TreePage page, int index, TreePageEntry parent)
        {
            Page = page;
            Index = index;
            Parent = parent;
        }

        public void Clear()
        {
            Page.Clear();
        }

        public void CopyTo(TreePage page)
        {
            Page.CopyTo(page);
        }

        public TreeNodeDataSlice GetNodeKey(int index)
        {
            return Page.GetNodeKey(index);
        }

        public TreeNodeDataSlice GetNodeData(int index)
        {
            return Page.GetNodeData(index);
        }

        public void Search(TreeNodeDataSlice key)
        {
            Page.Search(key);
        }

        public bool SearchPageIfBranch(ILowLevelTransaction tx, TreeNodeDataSlice key, out TreePage page)
        {
            return Page.SearchPageIfBranch(tx, key, out page);
        }

        public int CopyEntriesToNewPage(int index, TreePage newPage)
        {
            return Page.CopyEntriesToNewPage(index, newPage);
        }

        public void RemoveNode(int index)
        {
            Page.RemoveNode(index);
        }

        public void RemoveNode(int index, long txNumber, long txLogNumber)
        {
            Page.RemoveNode(index, txNumber, txLogNumber);
        }

        public bool Allocate(int index, ushort size, TreeNodeHeaderFlags flags, out ushort position)
        {
            return Page.Allocate(index, size, flags, out position);
        }

        public void InsertDataNode(int index, ushort pos, TreeNodeDataEntry entry, long txNumber)
        {
            Page.InsertDataNode(index, pos, entry, txNumber);
        }

        public void InsertDataRefNode(int index, ushort pos, TreeNodeDataEntry entry)
        {
            Page.InsertDataRefNode(index, pos, entry);
        }

        public void InsertPageRefNode(int index, ushort pos, TreeNodeDataSlice keySlice, long pageNumber)
        {
            Page.InsertPageRefNode(index, pos, keySlice, pageNumber);
        }
    }
}
