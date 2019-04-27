namespace Vicuna.Storage.Paging
{
    public interface IPageBufferPool
    {
        uint Limit { get; }

        Page GetEntry(PageNumberInfo page);

        void AddEntry(PageEntry entry);

        void AddEntry(Page page, PageEntryState state);
    }
}
