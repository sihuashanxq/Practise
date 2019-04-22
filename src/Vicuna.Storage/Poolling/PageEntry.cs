namespace Vicuna.Storage.Paging
{
    public struct PageEntry
    {
        public Page Page;

        public int Version;

        public PageEntryState State;

        public PageEntry(Page page)
        {
            Version = 0;
            Page = page;
            State = PageEntryState.None;
        }

        public PageEntry(Page page, int version, PageEntryState state = PageEntryState.None)
        {
            Page = page;
            State = state;
            Version = version;
        }
    }
}
