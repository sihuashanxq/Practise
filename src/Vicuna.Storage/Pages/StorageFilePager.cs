using System;
using System.Collections.Generic;
using System.IO;

namespace Vicuna.Storage.Pages.MMap
{
    public class StorageFilePager : StoragePageManager
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

                header->PageOffset = _maxAllocatedPage;
                header->PrePageOffset = -1;
                header->NextPageOffset = -1;
                header->FreeSize = Constants.PageSize - Constants.PageHeaderSize;
                header->PageSize = Constants.PageSize;
                header->ItemCount = 0;
                header->Flag = (byte)PageHeaderFlag.None;
                header->LastUsedPos = Constants.PageHeaderSize;

                _maxAllocatedPage++;

                Pages[header->PageOffset] = buffer;

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

        public unsafe virtual byte[] GetBuffer(long pageOffset)
        {
            if (pageOffset < 0)
            {
                throw new IndexOutOfRangeException(nameof(pageOffset));
            }

            if (StorageFile == null)
            {
                throw new InvalidOperationException("pager not be initialized!");
            }

            byte[] content = null;

            if (Pages.ContainsKey(pageOffset))
            {
                content = Pages[pageOffset];
            }
            else if (ReadPage(pageOffset * PageSize, out content) != PageSize)
            {
                throw new InvalidDataException($"read page id:{pageOffset} error!");
            }

            Pages[pageOffset] = content;

            fixed (byte* pointer = content)
            {
                var header = (PageHeader*)pointer;
                if (header->ModifiedCount == 0)
                {
                    header->PageOffset = pageOffset;
                    header->PrePageOffset = -1;
                    header->NextPageOffset = -1;
                    header->FreeSize = Constants.PageSize - Constants.PageHeaderSize;
                    header->PageSize = Constants.PageSize;
                    header->ItemCount = 0;
                    header->Flag = (byte)PageHeaderFlag.None;
                    header->LastUsedPos = Constants.PageHeaderSize;
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
            return Allocate(1)[0];
        }

        public override long[] Allocate(int pageCount)
        {
            var start = Create(pageCount);
            var rets = new long[pageCount];
            for (var i = 0; i < pageCount; i++)
            {
                rets[i] = start + i;
            }

            return rets;
        }

        public override byte[] GetPageContent(long pageOffset)
        {
            return GetPage(pageOffset).Buffer;
        }

        public override void FreePage(Page page)
        {
            throw new NotImplementedException();
        }

        public override void FreePage(byte[] pageContent)
        {
            throw new NotImplementedException();
        }
    }
}
