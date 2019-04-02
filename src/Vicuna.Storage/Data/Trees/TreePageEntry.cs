namespace Vicuna.Storage.Data.Trees
{
    public class TreePageEntry
    {
        public int Index { get; set; }

        public TreePage Page { get; set; }

        public TreePageEntry(int index, TreePage page)
        {
            Page = page;
            Index = index;
        }
    }
}
