namespace Vicuna.Storage.Data.Trees
{
    public class TreeState
    {
        private TreeRootHeader _header;

        public bool IsChanged { get; set; }

        public ref TreeRootHeader Header => ref _header;

        public TreeState(TreeRootHeader header)
        {
            _header = header;
        }

        public void OnChanged(TreePage page, TreePageChangeFlags flags)
        {
            switch (flags)
            {
                case TreePageChangeFlags.New:
                    if (page.IsBranch)
                    {
                        Header.PageCount++;
                        Header.BranchCount++;
                        break;
                    }

                    Header.PageCount++;
                    Header.LeafCount++;
                    break;
                case TreePageChangeFlags.Free:
                    if (page.IsBranch)
                    {
                        Header.PageCount--;
                        Header.BranchCount--;
                        break;
                    }

                    Header.PageCount--;
                    Header.LeafCount--;
                    break;
            }

            IsChanged = true;
        }

        public void OnChanged(OverflowPage page, TreePageChangeFlags flags)
        {
            switch (flags)
            {
                case TreePageChangeFlags.New:
                    Header.PageCount++;
                    Header.OverflowCount++;
                    break;
                case TreePageChangeFlags.Free:
                    Header.PageCount--;
                    Header.OverflowCount--;
                    break;
            }
        }
    }

    public enum TreePageChangeFlags
    {
        New,

        Free,

        Modify
    }
}
