using System;
using System.Collections.Generic;
using Vicuna.Storage.Stores;

namespace Vicuna.Storage.Paging.Impl
{
    public class Pager : IPager
    {
        public virtual int Id { get; }

        public virtual object SyncRoot { get; }

        public virtual IFileStore Store { get; }

        public virtual long Count => Store.Length / Constants.PageSize;

        public Pager(int id, IFileStore store)
        {
            Id = id;
            Store = store;
            SyncRoot = new object();
        }

        public virtual long AddPage(uint count)
        {
            lock (SyncRoot)
            {
                var old = Count;

                Store.SetLength(Store.Length + count * Constants.PageSize);

                return old;
            }
        }

        public virtual void FreePage(long pageNumber)
        {

        }

        public virtual byte[] GetPage(long pageNumber)
        {
            var pos = pageNumber * Constants.PageSize;
            var len = Constants.PageSize;

            return Store.ReadBytes(pos, len);
        }

        public virtual void SetPage(long pageNumber, byte[] src)
        {
            var pos = pageNumber * Constants.PageSize;

            Store.WriteBytes(pos, src);
        }

        public virtual PageIdentity Allocate()
        {
            return new PageIdentity(Id, AddPage(1));
        }

        public virtual PageIdentity[] Allocate(uint count)
        {
            var start = AddPage(count);
            var pages = new PageIdentity[count];

            for (var i = 0; i < count; i++)
            {
                pages[i] = new PageIdentity(Id, start + i);
            }

            return pages;
        }

        public virtual void FreePage(IEnumerable<long> pageNumbers)
        {
            if (pageNumbers == null)
            {
                return;
            }

            foreach (var item in pageNumbers)
            {
                FreePage(item);
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
