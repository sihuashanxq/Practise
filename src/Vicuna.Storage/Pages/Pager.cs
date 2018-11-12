using System;

namespace Vicuna.Storage.Pages
{
    public abstract class Pager : IDisposable
    {
        public virtual long Count { get; }

        public virtual long Length { get; }

        public virtual int PageSize { get; }

        public virtual long MaxAllocatedPage { get; }

        public abstract Page Create();

        public abstract Page GetPage(long id);

        public abstract void FreePage(Page page);

        public virtual void Dispose()
        {

        }
    }
}