using System;
using System.IO;
using System.Threading;

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

        public MemoryMappedFilePager(long maxAllocatedPage, MemoryMappedFileInfo file)
        {
            _maxAllocatedPage = maxAllocatedPage;

            File = file;
            Length = File.Size;
            Count = File.Size % PageSize == 0 ? File.Size / PageSize : (File.Size / PageSize + 1);
        }

        public unsafe override Page Create()
        {
            var buffer = new byte[PageSize];

            fixed (byte* pointer = buffer)
            {
                var header = (PageHeader*)pointer;

                header->PagePos = Interlocked.Add(ref _maxAllocatedPage, 1);
                header->PrePagePos = -1;
                header->NextPagePos = -1;
                header->FreeSize = Constants.PageSize - Constants.PageHeaderSize;
                header->PageSize = Constants.PageSize;
                header->ItemCount = 0;
                header->Flag = (byte)PageHeaderFlag.None;
                header->LastUsedPos = Constants.PageHeaderSize;

                return new Page(buffer);
            }
        }

        public override void FreePage(Page page)
        {
            throw new NotImplementedException();
        }

        public override Page GetPage(long pageid)
        {
            if (pageid > Count || pageid < 0)
            {
                throw new InvalidDataException("page offset out of data file size!");
            }

            if (File == null || File.Stream == null)
            {
                throw new InvalidOperationException("pager not be initialized!");
            }

            if (ReadPage(pageid * PageSize, out var content) != PageSize)
            {
                throw new InvalidDataException($"read page id:{pageid} error!");
            }

            return new Page(content);
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
