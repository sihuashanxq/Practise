using System;

namespace Vicuna.Storage.Pages
{
    public abstract class Pager : IDisposable
    {
        public virtual int PageSize { get; }

        public virtual long MaxAllocatedPage { get; }

        public abstract Page Create();

        public abstract long Create(int count);

        public abstract Page GetPage(long pos);

        public abstract byte[] GetBuffer(long pos);

        public virtual void Dispose()
        {

        }
    }
}