using System;
using System.Threading;

namespace Vicuna.Storage.Pages
{
    public unsafe class StorageFilePageManager
    {
        private long _maxPageNumber;

        private StorageFile _storageFile;

        public long MaxAllocatedPage => _maxPageNumber;

        public StorageFile StorageFile => _storageFile;

        public StorageFilePageManager(long maxPageNumber, StorageFile storageFile)
        {
            _storageFile = storageFile;
            _maxPageNumber = maxPageNumber;
        }

        public virtual long AppendPage(int pageCount)
        {
            return Math.Min(_maxPageNumber, Interlocked.Add(ref _maxPageNumber, pageCount));
        }

        public virtual byte[] GetPage(long pageNumber)
        {
            if (pageNumber < 0)
            {
                throw new IndexOutOfRangeException(nameof(pageNumber));
            }

            var buffer = StorageFile.Read(pageNumber * Constants.PageSize, Constants.PageSize);
            if (buffer == null)
            {
                throw new InvalidOperationException(nameof(buffer));
            }

            fixed (byte* pointer = buffer)
            {
                var header = (PageHeader*)pointer;
                if (header->PageNumber != pageNumber)
                {
                    header->PageNumber = pageNumber;
                    header->PrePageNumber = -1;
                    header->NextPageNumber = -1;
                    header->FreeSize = Constants.PageSize - Constants.PageHeaderSize;
                    header->PageSize = Constants.PageSize;
                    header->ItemCount = 0;
                    header->Flag = (byte)PageHeaderFlag.None;
                    header->LastUsedIndex = Constants.PageHeaderSize;
                }
            }

            return buffer;
        }

        public virtual void SetPage(long pageNumber, byte[] page)
        {
            if (pageNumber < 0)
            {
                throw new IndexOutOfRangeException(nameof(pageNumber));
            }

            if (page == null)
            {
                throw new NullReferenceException(nameof(page));
            }

            StorageFile.Write(pageNumber * Constants.PageSize, page);
        }

        public virtual void Dispose()
        {
            StorageFile.Dispose();
        }
    }
}
