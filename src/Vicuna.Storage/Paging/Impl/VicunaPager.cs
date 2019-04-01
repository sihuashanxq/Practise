using System;

namespace Vicuna.Storage.Paging.Impl
{
    public class VicunaPager : IPager
    {
        public virtual int Id => throw new NotImplementedException();

        public virtual long Count => throw new NotImplementedException();

        public virtual IPageAllocator Allocator => throw new NotImplementedException();

        public virtual void AddPage(uint count)
        {
            throw new NotImplementedException();
        }

        public virtual void Flush(bool flushToDisk)
        {
            throw new NotImplementedException();
        }

        public byte[] GetPageData(long pageNumber)
        {
            if (pageNumber > Count - 1 || pageNumber < 0)
            {
                throw new ArgumentOutOfRangeException($"page number {pageNumber} out of the pager's range");
            }

            return GetPageDataInternal(pageNumber);
        }

        public void SetPageData(long pageNumber, byte[] data)
        {
            if (pageNumber > Count - 1 || pageNumber < 0)
            {
                throw new ArgumentOutOfRangeException($"page number {pageNumber} out of the pager's range");
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            SetPageDataInternal(pageNumber, data);
        }

        protected virtual void SetPageDataInternal(long pageNumber, byte[] data)
        {
            throw new NotImplementedException();
        }

        protected virtual byte[] GetPageDataInternal(long pageNumber)
        {
            throw new NotImplementedException();
        }
    }
}
