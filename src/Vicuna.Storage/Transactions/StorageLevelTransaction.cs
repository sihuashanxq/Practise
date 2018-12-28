using Vicuna.Storage.Pages;

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

        internal StorageLevelTransactionBufferManager Buffer { get; }

        public StorageLevelTransaction(StorageLevelTransactionBufferManager buffer)
        {
            Buffer = buffer;
        }

        public bool AllocatePage(out Page page)
        {
            if (!_slice.AllocatePage(out page))
            {

            }

            return false;
        }

        public long AllocateSlicePage()
        {
            return Buffer.AllocateSlicePage();
        }

        public Page GetPage(long pageOffset)
        {
            if (Buffer.TryGetPage(pageOffset, out var page))
            {
                return page;
            }

            return null;
        }

        public Page GetPageToModify(long pageOffset)
        {
            if (Buffer.TryGetPageToModify(pageOffset, out var modifedPage))
            {
                return modifedPage;
            }

            return null;
        }
    }
}
