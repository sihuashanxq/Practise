namespace Vicuna.Storage.Pages
{
    public abstract class StoragePaginationManager
    {
        public abstract long Allocate();

        public abstract long Allocate(int pageCount);

        public abstract byte[] GetPageContent(long pageNumber);

        public abstract void FreePage(byte[] pageContent);

        public virtual void Dispose()
        {

        }
    }
}
