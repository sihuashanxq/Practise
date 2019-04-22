using System.Collections.Generic;

namespace Vicuna.Storage.Paging
{
    public interface IPageAllocator
    {
        PageIdentity Allocate();

        PageIdentity[] Allocate(uint count);

        void FreePage(long pageNumber);

        void FreePage(IEnumerable<long> pageNumbers);
    }
}
