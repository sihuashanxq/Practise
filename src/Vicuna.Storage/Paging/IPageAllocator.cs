using System.Collections.Generic;

namespace Vicuna.Storage.Paging
{
    public interface IPageAllocator
    {
        PageNumberInfo Allocate();

        PageNumberInfo[] Allocate(uint count);

        void FreePage(long pageNumber);

        void FreePage(IEnumerable<long> pageNumbers);
    }
}
