using System;

namespace Vicuna.Storage.Stores.Paging
{
    public class StorePagerNotFoundException : Exception
    {
        public int StoreId { get; }

        public StorePagerNotFoundException(int storeId) : base($"can't find a pager,storeId:{storeId} ")
        {
            StoreId = storeId;
        }
    }
}
