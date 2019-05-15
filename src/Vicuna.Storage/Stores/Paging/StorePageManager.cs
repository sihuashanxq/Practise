using System;
using System.Collections.Generic;
using System.Linq;
using Vicuna.Storage.Paging;

namespace Vicuna.Storage.Stores.Paging
{
    public class StorePageManager : IStorePageManager
    {
        public IStorePagerProvider Provider { get; }

        public StorePageManager(IStorePagerProvider provider)
        {
            Provider = provider;
        }

        public PageNumberInfo Allocate(int storeId)
        {
            var pager = Provider.GetPager(storeId);
            if (pager == null)
            {
                throw new NullReferenceException(nameof(pager));
            }

            return pager.Allocate();
        }

        public PageNumberInfo[] Allocate(int storeId, uint count)
        {
            var pager = Provider.GetPager(storeId);
            if (pager == null)
            {
                throw new NullReferenceException(nameof(pager));
            }

            return pager.Allocate(count);
        }

        public void SetPage(PageNumberInfo number, Page page)
        {
            var pager = Provider.GetPager(number.StoreId);
            if (pager == null)
            {
                throw new NullReferenceException(nameof(pager));
            }

            pager.SetPage(number.PageNumber, page.Data);
        }

        public Page GetPage(PageNumberInfo number)
        {
            var pager = Provider.GetPager(number.StoreId);
            if (pager == null)
            {
                throw new NullReferenceException(nameof(pager));
            }

            return new Page(pager.GetPage(number.PageNumber));
        }

        public void Free(PageNumberInfo page)
        {
            var pager = Provider.GetPager(page.StoreId);
            if (pager == null)
            {
                throw new NullReferenceException(nameof(pager));
            }
        }

        public void Free(IEnumerable<PageNumberInfo> pages)
        {
            foreach (var group in pages.GroupBy(i => i.StoreId))
            {
                var pager = Provider.GetPager(group.Key);
                if (pager == null)
                {
                    throw new NullReferenceException(nameof(pager));
                }

                pager.Free(group.Select(i => i.PageNumber));
            }
        }
    }
}
