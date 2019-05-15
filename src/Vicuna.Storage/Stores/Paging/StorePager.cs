using System;
using System.Collections.Generic;
using Vicuna.Storage.Paging;

namespace Vicuna.Storage.Stores.Paging
{
    public class StorePager : IStorePager
    {
        public virtual object SyncRoot => this;

        public virtual IFileStore Store { get; }

        public virtual long Count => Store.Length / Constants.PageSize;

        public StorePager(IFileStore store)
        {
            Store = store;
        }

        public virtual long AddPage(uint count)
        {
            lock (Store.SyncRoot)
            {
                var old = Count;

                Store.SetLength(Store.Length + count * Constants.PageSize);

                return old;
            }
        }

        public virtual void Free(long pageNumber)
        {

        }

        public virtual byte[] GetPage(long pageNumber)
        {
            return Store.Read(pageNumber * Constants.PageSize, Constants.PageSize);
        }

        public virtual void SetPage(long pageNumber, byte[] src)
        {
            Store.Write(pageNumber * Constants.PageSize, src);
        }

        public virtual PageNumberInfo Allocate()
        {
            return new PageNumberInfo(Store.Id, AddPage(1));
        }

        public virtual PageNumberInfo[] Allocate(uint count)
        {
            var start = AddPage(count);
            var pages = new PageNumberInfo[count];

            for (var i = 0; i < count; i++)
            {
                pages[i] = new PageNumberInfo(Store.Id, start + i);
            }

            return pages;
        }

        public virtual void Free(IEnumerable<long> pageNumbers)
        {
            if (pageNumbers == null)
            {
                return;
            }

            foreach (var item in pageNumbers)
            {
                Free(item);
            }
        }

        protected virtual uint GetPreAllocatedCount(uint count)
        {
            var size = Count * Constants.PageSize;
            if (size < 1024 * 1024)
            {
                return Math.Max(count, 8);      //128KB
            }

            if (size < 8 * 1024 * 1024)
            {
                return Math.Max(count, 16);     //256KB
            }

            if (size < 32 * 1024 * 1024)
            {
                return Math.Max(count, 32);     //512KB
            }

            if (size < 128 * 1024 * 1024)
            {
                return Math.Max(count, 128);    //2MB
            }

            if (size < 256 * 1024 * 1024)
            {
                return Math.Max(count, 256);    //4MB
            }

            if (size < 512 * 1024 * 1024)
            {
                return Math.Max(count, 1024);   //16MB
            }

            return Math.Max(count, 32);         //32MB
        }
    }
}
