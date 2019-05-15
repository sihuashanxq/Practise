using System.Collections.Generic;
using Vicuna.Storage.Paging;

namespace Vicuna.Storage.Stores.Paging
{
    public interface IStorePageAllocator
    {
        PageNumberInfo Allocate();

        PageNumberInfo[] Allocate(uint count);

        void Free(long pageNumber);

        void Free(IEnumerable<long> pageNumbers);
    }
}