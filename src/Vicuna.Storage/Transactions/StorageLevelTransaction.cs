namespace Vicuna.Storage.Transactions
{
    public class StorageLevelTransaction
    {
        internal StorageSlice Slice { get; }

        internal StorageSliceManager SliceManager { get; }

        internal StorageLevelTransactionPageBuffer Buffer { get; }

        public StorageLevelTransaction(StorageLevelTransactionPageBuffer buffer)
        {
            Buffer = buffer;
        }

        public bool AllocatePageFromSlice(out byte[] page)
        {
            page = null;
            return false;
        }

        public long[] AllocatePageFromBuffer(int pageCount)
        {
            return Buffer.AllocatePage(pageCount);
        }

        public byte[] GetPage(long pageOffset)
        {
            if (Buffer.TryGetPage(pageOffset, out var pageContent))
            {
                return pageContent;
            }

            return null;
        }

        public byte[] GetPageToModify(long pageOffset)
        {
            if (Buffer.TryGetPageToModify(pageOffset, out var pageContent))
            {
                return pageContent;
            }

            return null;
        }
    }
}
