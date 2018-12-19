namespace Vicuna.Storage.Transactions
{
    public class StorageLevelTransaction
    {
        internal StorageLevelTransactionPageBuffer PageBuffer { get; }

        public StorageLevelTransaction(StorageLevelTransactionPageBuffer pageBuffer)
        {
            PageBuffer = pageBuffer;
        }

        public long AllocatePage()
        {
            return PageBuffer.AllocatePage();
        }

        public long[] AllocatePage(int pageCount)
        {
            return PageBuffer.AllocatePage(pageCount);
        }

        public byte[] GetPage(long pageOffset)
        {
            if (PageBuffer.TryGetPage(pageOffset, out var pageContent))
            {
                return pageContent;
            }

            return null;
        }

        public byte[] GetPageToModify(long pageOffset)
        {
            if (PageBuffer.TryGetPageToModify(pageOffset, out var pageContent))
            {
                return pageContent;
            }

            return null;
        }
    }
}
