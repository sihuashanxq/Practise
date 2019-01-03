using Vicuna.Storage.Pages;

namespace Vicuna.Storage.Transactions
{
    public class StorageLevelTransaction
    {
        private StorageSlice _slice;

        internal StorageSlice Slice => _slice ?? (_slice = SliceManager.Allocate());

        internal StorageSliceManager SliceManager { get; }

        internal StorageSliceActivingList ActivedSlices { get; }

        internal StorageLevelTransactionBufferManager Buffer { get; }

        public StorageLevelTransaction(StorageLevelTransactionBufferManager buffer)
        {
            Buffer = buffer;
            ActivedSlices = new StorageSliceActivingList(this);
        }

        public bool AllocatePage(out Page page)
        {
            if (!_slice.AllocatePage(out page))
            {
                return false;
            }

            page = null;
            return false;
        }

        public long AllocateSlicePage()
        {
            return Buffer.TryAllocateSlicePage(out var pageOffset) ? pageOffset : -1;
        }

        public Page GetPage(long pageNumber)
        {
            return Buffer.TryGetPage(pageNumber, out var page) ? page : null;
        }

        public Page GetPageToModify(long pageNumber)
        {
            return Buffer.TryGetPageToModify(pageNumber, out var page) ? page : null;
        }
    }
}
