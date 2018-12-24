namespace Vicuna.Storage.Transactions
{
    public class StorageLevelTransaction
    {
        private StorageSlice _slice;

        internal StorageSlice Slice
        {
            get
            {
                if (_slice == null)
                {
                    _slice = SliceManager.Allocate();
                }

                return _slice;
            }
        }

        internal StorageSliceManager SliceManager { get; }

        internal StorageLevelTransactionPageBuffer Buffer { get; }

        public StorageLevelTransaction(StorageLevelTransactionPageBuffer buffer)
        {
            Buffer = buffer;
        }

        public bool AllocatePageFromSlice(out byte[] page)
        {
            if (!_slice.AllocatePage(out var x))
            {

            }

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
