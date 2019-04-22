namespace Vicuna.Storage.Data.Trees
{
    public class TreePageEntry
    {
        public int Index { get; set; }

        public TreePage Page { get; set; }

        public TreePageEntry Parent { get; set; }

        public TreePageEntry(TreePage page, int index, TreePageEntry parent)
        {
            Page = page;
            Index = index;
            Parent = parent;
        }
    }
}
