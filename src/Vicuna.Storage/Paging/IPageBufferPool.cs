
namespace Vicuna.Storage.Paging
{
    public interface IPageBufferPool
    {
        uint Limit { get; }

        Page GetEntry(PageIdentity identity);

        void SetEntry(PageEntry bufferEntry);

        void SetEntry(Page page, int version, PageEntryState state);
    }
}
