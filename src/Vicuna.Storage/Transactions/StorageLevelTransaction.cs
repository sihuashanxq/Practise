using System;
using Vicuna.Storage.Pages;

namespace Vicuna.Storage.Transactions
{
    public class StorageLevelTransaction
    {
        private StorageSlice _lastUsedSlice;

        private readonly StorageSliceActivingList _activedSlices;

        private readonly StorageSliceManager _storageSliceManager;

        private readonly StorageLevelTransactionBufferManager _buffer;

        internal StorageSlice LastUsedSlice
        {
            get
            {
                if (_lastUsedSlice == null)
                {
                    _lastUsedSlice = _storageSliceManager.CreateSlice();
                    _activedSlices.Insert(_lastUsedSlice);
                }

                return _lastUsedSlice;
            }
        }

        internal StorageLevelTransactionBufferManager Buffer => _buffer;

        internal StorageSliceActivingList ActivedSlices => _activedSlices;

        internal StorageSliceManager StorageSliceManager => _storageSliceManager;

        public StorageLevelTransaction(StorageLevelTransactionBufferManager buffer)
        {
            _buffer = buffer;
            _activedSlices = new StorageSliceActivingList(this);
            _storageSliceManager = new StorageSliceManager(this);
        }

        public bool Allocate(int size, out PageSlice pageSlice)
        {
            if (LastUsedSlice.Allocate(size, out pageSlice))
            {
                return true;
            }

            if (AllocateFromActivedSlice(size, out pageSlice))
            {
                return true;
            }

            _lastUsedSlice?.Dispose();
            _lastUsedSlice = StorageSliceManager.CreateSlice();
            _activedSlices.Insert(_lastUsedSlice);

            return _lastUsedSlice.Allocate(size, out pageSlice);
        }

        public bool AllocatePage(out Page newPage)
        {
            if (LastUsedSlice.AllocatePage(out newPage))
            {
                return true;
            }

            if (AllocatePageFromActivedSlice(out newPage))
            {
                return true;
            }

            _lastUsedSlice?.Dispose();
            _lastUsedSlice = StorageSliceManager.CreateSlice();
            _activedSlices.Insert(_lastUsedSlice);

            return _lastUsedSlice.AllocatePage(out newPage);
        }

        public bool AllocateFromActivedSlice(int size, out PageSlice pageSlice)
        {
            foreach (var item in ActivedSlices)
            {
                var entry = item.FirstEntry;
                if (entry.Usage.PageNumber == -1 ||
                    entry.Usage.FreePageCount == 0)
                {
                    continue;
                }

                var newSlice = StorageSliceManager.GetSlice(entry.Usage.PageNumber);
                if (newSlice.Allocate(size, out pageSlice))
                {
                    _lastUsedSlice.Dispose();
                    _lastUsedSlice = newSlice;
                    return true;
                }
            }

            pageSlice = null;
            return false;
        }

        public bool AllocatePageFromActivedSlice(out Page newPage)
        {
            foreach (var item in ActivedSlices)
            {
                var entry = item.FirstEntry;
                if (entry.Usage.PageNumber == -1 ||
                    entry.Usage.FreePageCount == 0)
                {
                    continue;
                }

                var newSlice = StorageSliceManager.GetSlice(entry.Usage.PageNumber);
                if (newSlice.AllocatePage(out newPage))
                {
                    _lastUsedSlice.Dispose();
                    _lastUsedSlice = newSlice;
                    return true;
                }
            }

            newPage = null;
            return false;
        }

        public long AllocateSlicePage()
        {
            return Buffer.TryAllocateSlicePage(out var pageNumber) ? pageNumber : -1;
        }

        public Page GetPage(long pageNumber)
        {
            return Buffer.TryGetPage(pageNumber, out var page) ? page : null;
        }

        public Page GetPageToModify(long pageNumber)
        {
            return Buffer.TryGetPageToModify(pageNumber, out var page) ? page : null;
        }

        private void CeateSlice()
        {

        }
    }
}
