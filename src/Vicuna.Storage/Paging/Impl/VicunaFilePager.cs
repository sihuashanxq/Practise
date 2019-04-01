using System;
using System.IO;

namespace Vicuna.Storage.Paging.Impl
{
    public class VicunaFilePager : VicunaPager
    {
        public override int Id { get; }

        public FileStream FileStream { get; }

        public override IPageAllocator Allocator { get; }

        public override long Count => FileStream.Length / Constants.PageSize;

        public VicunaFilePager(int id, FileStream fileStream)
        {
            Id = id;
            Allocator = new VicunaPageAllcator(this);
            FileStream = fileStream ?? throw new ArgumentNullException(nameof(fileStream));
        }

        public override void AddPage(uint count)
        {
            FileStream.SetLength(FileStream.Length + count * Constants.PageSize);
        }

        public override void Flush(bool flushToDisk)
        {
            FileStream.Flush(flushToDisk);
        }

        protected override byte[] GetPageDataInternal(long pageNumber)
        {
            var data = new byte[Constants.PageSize];

            FileStream.Seek(Constants.PageSize * pageNumber, SeekOrigin.Begin);
            FileStream.Read(data);

            return data;
        }

        protected override void SetPageDataInternal(long pageNumber, byte[] data)
        {
            FileStream.Seek(Constants.PageSize * pageNumber, SeekOrigin.Begin);
            FileStream.Write(data, 0, data.Length);
        }
    }
}
