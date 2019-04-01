using System.Collections.Generic;

namespace Vicuna.Storage.Paging
{
    public interface IPageAllocator
    {
        IPager Pager { get; }

        PageIdentity Allocate();

        PageIdentity[] Allocate(uint count);

        void Free(long pageNumber);

        void Free(IEnumerable<long> pageNumbers);

        void Free(PageIdentity page);

        void Free(IEnumerable<PageIdentity> pages);
    }
}
