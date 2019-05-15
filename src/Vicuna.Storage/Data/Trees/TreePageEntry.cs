using System;
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

        public Span<byte> MinKey => Page.MinKey;

        public Span<byte> MaxKey => Page.MaxKey;

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

        public Span<byte> GetNodeKey(int index)
        {
            return Page.GetNodeKey(index);
        }

        public Span<byte> GetNodeData(int index)
        {
            return Page.GetNodeData(index);
        }

        public int SearchForKey(Span<byte> key)
        {
            Search(key);

            if (LastMatch < 0)
            {
                LastMatchIndex++;
            }

            return LastMatch;
        }

        public void Search(Span<byte> key)
        {
            Page.Search(key);
        }

        public bool SearchPageIfBranch(ILowLevelTransaction tx, Span<byte> key, out TreePage page)
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

        public void InsertDataNode(int index, ushort pos, Span<byte> k, Span<byte> v, long txNumber)
        {
            Page.AddEntry(index, pos, k, v, txNumber);
        }

        public void InsertDataRefNode(int index, ushort pos, Span<byte> k, Span<byte> v)
        {
            Page.AddEntryRef(index, pos, k, v);
        }

        public void InsertPageRefNode(int index, ushort pos, Span<byte> k, long pageNumber)
        {
            Page.AddEntryPageRef(index, pos, k, pageNumber);
        }
    }
}
