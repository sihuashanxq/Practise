namespace Vicuna.Storage.Pages
{
    public abstract class StoragePageManager
    {
        public abstract long Allocate();

        public abstract long[] Allocate(int pageCount);

        public abstract byte[] GetPageContent(long pageOffset);

        public abstract void FreePage(Page page);

        public abstract void FreePage(byte[] pageContent);

        public virtual void Dispose()
        {

        }
    }
}
