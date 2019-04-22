namespace Vicuna.Storage.Paging
{
    public interface IPageBufferPool
    {
        uint Limit { get; }

        Page GetEntry(PageIdentity page);

        void AddEntry(PageEntry entry);

        void AddEntry(Page page, PageEntryState state);
    }
}
