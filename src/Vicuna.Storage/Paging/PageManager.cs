using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Vicuna.Storage.Paging.Impl
{
    public class PageManager : IPageManager
    {
        public readonly ConcurrentDictionary<int, Pager> _pagers;

        public PageManager()
        {
            _pagers = new ConcurrentDictionary<int, Pager>();
        }

        public PageNumberInfo Allocate(int pagerId)
        {
            if (!_pagers.TryGetValue(pagerId, out var pager) || pager == null)
            {
                throw new PagerNotFoundException(pagerId);
            }

            return pager.Allocate();
        }

        public PageNumberInfo[] Allocate(int pagerId, uint count)
        {
            if (!_pagers.TryGetValue(pagerId, out var pager) || pager == null)
            {
                throw new PagerNotFoundException(pagerId);
            }

            return pager.Allocate(count);
        }

        public void SetPage(PageNumberInfo number, Page page)
        {
            if (!_pagers.TryGetValue(number.StoreId, out var pager) || pager == null)
            {
                throw new PagerNotFoundException(number.StoreId);
            }

            pager.SetPage(number.PageNumber, page.Data);
        }

        public Page GetPage(PageNumberInfo number)
        {
            if (!_pagers.TryGetValue(number.StoreId, out var pager) || pager == null)
            {
                throw new PagerNotFoundException(number.StoreId);
            }

            return new Page(pager.GetPage(number.PageNumber));
        }

        public void FreePage(PageNumberInfo page)
        {
            if (!_pagers.TryGetValue(page.StoreId, out var pager) || pager == null)
            {
                throw new PagerNotFoundException(page.StoreId);
            }
        }

        public void FreePage(IEnumerable<PageNumberInfo> pages)
        {
            foreach (var group in pages.GroupBy(i => i.StoreId))
            {
                if (!_pagers.TryGetValue(group.Key, out var pager) || pager == null)
                {
                    throw new PagerNotFoundException(group.Key);
                }

                pager.FreePage(group.Select(i => i.PageNumber));
            }
        }
    }
}
