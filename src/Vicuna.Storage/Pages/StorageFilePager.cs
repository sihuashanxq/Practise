using System;
using System.Collections.Generic;
using System.IO;

namespace Vicuna.Storage.Pages.MMap
{
    public class StorageFilePager : Pager
    {
        private long _maxAllocatedPage;

        public override long MaxAllocatedPage => _maxAllocatedPage;

        public override int PageSize { get; } = Constants.PageSize;

        public StorageFile StorageFile { get; }

        public Dictionary<long, byte[]> Pages { get; }

        public StorageFilePager(long maxAllocatedPage, StorageFile file)
        {
            _maxAllocatedPage = maxAllocatedPage;
            StorageFile = file;
            Pages = new Dictionary<long, byte[]>();
        }

        public unsafe override Page Create()
        {
            var buffer = new byte[PageSize];

            fixed (byte* pointer = buffer)
            {
                var header = (PageHeader*)pointer;

                header->PagePos = _maxAllocatedPage;
                header->PrePagePos = -1;
                header->NextPagePos = -1;
                header->FreeSize = Constants.PageSize - Constants.PageHeaderSize;
                header->PageSize = Constants.PageSize;
                header->ItemCount = 0;
                header->Flag = (byte)PageHeaderFlag.None;
                header->LastUsedPos = Constants.PageHeaderSize;

                _maxAllocatedPage++;

                Pages[header->PagePos] = buffer;

                return new Page(buffer);
            }
        }

        public override long Create(int count)
        {
            var pos = _maxAllocatedPage;
            _maxAllocatedPage += count;
            return pos;
        }

        public override Page GetPage(long pos)
        {
            return new Page(GetBuffer(pos));
        }

        public unsafe override byte[] GetBuffer(long pageOffset)
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
                    header->PagePos = pageOffset;
                    header->PrePagePos = -1;
                    header->NextPagePos = -1;
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

        public override void Dispose()
        {
            //flush
            StorageFile.Dispose();
        }
    }
}
