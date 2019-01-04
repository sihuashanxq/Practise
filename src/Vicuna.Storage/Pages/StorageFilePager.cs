using System;
using System.Collections.Generic;
using System.IO;

namespace Vicuna.Storage.Pages.MMap
{
    public class StorageFilePager : StoragePaginationManager
    {
        private long _maxAllocatedPage;

        public virtual long MaxAllocatedPage => _maxAllocatedPage;

        public virtual int PageSize { get; } = Constants.PageSize;

        public StorageFile StorageFile { get; }

        public Dictionary<long, byte[]> Pages { get; }

        public StorageFilePager(long maxAllocatedPage, StorageFile file)
        {
            _maxAllocatedPage = maxAllocatedPage;
            StorageFile = file;
            Pages = new Dictionary<long, byte[]>();
        }

        public unsafe virtual Page Create()
        {
            var buffer = new byte[PageSize];

            fixed (byte* pointer = buffer)
            {
                var header = (PageHeader*)pointer;

                header->PageNumber = _maxAllocatedPage;
                header->PrePageNumber = -1;
                header->NextPageNumber = -1;
                header->FreeSize = Constants.PageSize - Constants.PageHeaderSize;
                header->PageSize = Constants.PageSize;
                header->ItemCount = 0;
                header->Flag = (byte)PageHeaderFlag.None;
                header->LastUsedIndex = Constants.PageHeaderSize;

                _maxAllocatedPage++;

                Pages[header->PageNumber] = buffer;

                return new Page(buffer);
            }
        }

        public virtual long Create(int count)
        {
            var pos = _maxAllocatedPage;
            _maxAllocatedPage += count;
            return pos;
        }

        public virtual Page GetPage(long pos)
        {
            return new Page(GetBuffer(pos));
        }

        public unsafe virtual byte[] GetBuffer(long pageNumber)
        {
            if (pageNumber < 0)
            {
                throw new IndexOutOfRangeException(nameof(pageNumber));
            }

            if (StorageFile == null)
            {
                throw new InvalidOperationException("pager not be initialized!");
            }

            byte[] content = null;

            if (Pages.ContainsKey(pageNumber))
            {
                content = Pages[pageNumber];
            }
            else if (ReadPage(pageNumber * PageSize, out content) != PageSize)
            {
                throw new InvalidDataException($"read page id:{pageNumber} error!");
            }

            Pages[pageNumber] = content;

            fixed (byte* pointer = content)
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

            return content;
        }

        protected virtual int ReadPage(long offset, out byte[] content)
        {
            content = StorageFile.Read(offset, Constants.PageSize);

            return content.Length;
        }

        public virtual void Dispose()
        {
            //flush
            StorageFile.Dispose();
        }

        public override long Allocate()
        {
            return Allocate(1);
        }

        public override long Allocate(int pageCount)
        {
            return Create(pageCount);
        }

        public override byte[] GetPageContent(long pageNumber)
        {
            return GetPage(pageNumber).Buffer;
        }

        public override void FreePage(byte[] pageContent)
        {
            throw new NotImplementedException();
        }
    }
}
