using System;
using System.Collections.Generic;
using Vicuna.Storage.Paging;

namespace Vicuna.Storage.Stores.Paging
{
    public class StorePagerProvider : IStorePagerProvider
    {
        private readonly IReadOnlyDictionary<int, IStorePager> _storePagers;

        public StorePagerProvider(IReadOnlyDictionary<int, IStorePager> storePagers)
        {
            _storePagers = storePagers ?? throw new ArgumentNullException(nameof(storePagers));
        }

        public IStorePager GetPager(int storeId)
        {
            if (!_storePagers.TryGetValue(storeId, out var pager))
            {
                throw new StorePagerNotFoundException(storeId);
            }

            return pager;
        }
    }
}
