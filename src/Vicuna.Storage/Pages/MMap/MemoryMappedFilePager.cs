using System;
using System.Collections.Generic;
using System.IO;

namespace Vicuna.Storage.Pages.MMap
{
    public class MemoryMappedFilePager : Pager
    {
        private long _maxAllocatedPage;

        public override long MaxAllocatedPage => _maxAllocatedPage;

        public override long Count { get; }

        public override long Length { get; }

        public override int PageSize { get; } = Constants.PageSize;

        public MemoryMappedFileInfo File { get; }

        public Dictionary<long, byte[]> Pages { get; }

        public MemoryMappedFilePager(long maxAllocatedPage, MemoryMappedFileInfo file)
        {
            _maxAllocatedPage = maxAllocatedPage;

            File = file;
            Length = File.Size;
            Pages = new Dictionary<long, byte[]>();
            Count = File.Size % PageSize == 0 ? File.Size / PageSize : (File.Size / PageSize + 1);
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

        public override void FreePage(Page page)
        {
            throw new NotImplementedException();
        }

        public override Page GetPage(long pos)
        {
            return new Page(GetBuffer(pos));
        }

        public unsafe override byte[] GetBuffer(long pos)
        {
            if (pos > Count || pos < 0)
            {
                throw new InvalidDataException("page offset out of data file size!");
            }

            if (File == null || File.Stream == null)
            {
                throw new InvalidOperationException("pager not be initialized!");
            }

            byte[] content = null;

            if (Pages.ContainsKey(pos))
            {
                content = Pages[pos];
            }

            else if (ReadPage(pos * PageSize, out content) != PageSize)
            {
                throw new InvalidDataException($"read page id:{pos} error!");
            }

            Pages[pos] = content;

            fixed (byte* pointer = content)
            {
                var header = (PageHeader*)pointer;
                if (header->ModifiedCount == 0)
                {
                    header->PagePos = pos;
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
            if (offset > Length - PageSize)
            {
                throw new InvalidDataException("page offset out of data file size!");
            }

            content = new byte[PageSize];

            File.Stream.Seek(offset, SeekOrigin.Begin);

            return File.Stream.Read(content);
        }

        public override void Dispose()
        {
            //flush
            File.Dispose();
        }
    }
}
