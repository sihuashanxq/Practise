using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Vicuna.Storage.Paging.Impl
{
    public class PageManager : IPageManager
    {
        public readonly ConcurrentDictionary<int, Pager> _pagerMaps;

        public PageManager()
        {
            _pagerMaps = new ConcurrentDictionary<int, Pager>();
        }

        public PageIdentity Allocate(int pagerId)
        {
            if (!_pagerMaps.TryGetValue(pagerId, out var pager) || pager == null)
            {
                throw new PagerNotFoundException(pagerId);
            }

            return pager.Allocate();
        }

        public PageIdentity[] Allocate(int pagerId, uint count)
        {
            if (!_pagerMaps.TryGetValue(pagerId, out var pager) || pager == null)
            {
                throw new PagerNotFoundException(pagerId);
            }

            return pager.Allocate(count);
        }

        public void SetPage(PageIdentity identity, Page page)
        {
            if (!_pagerMaps.TryGetValue(identity.PagerId, out var pager) || pager == null)
            {
                throw new PagerNotFoundException(identity.PagerId);
            }

            pager.SetPage(identity.PageNumber, page.Data);
        }

        public Page GetPage(PageIdentity identity)
        {
            if (!_pagerMaps.TryGetValue(identity.PagerId, out var pager) || pager == null)
            {
                throw new PagerNotFoundException(identity.PagerId);
            }

            return new Page(pager.GetPage(identity.PageNumber));
        }

        public void FreePage(PageIdentity page)
        {
            if (!_pagerMaps.TryGetValue(page.PagerId, out var pager) || pager == null)
            {
                throw new PagerNotFoundException(page.PagerId);
            }
        }

        public void FreePage(IEnumerable<PageIdentity> pages)
        {
            foreach (var group in pages.GroupBy(i => i.PagerId))
            {
                if (!_pagerMaps.TryGetValue(group.Key, out var pager) || pager == null)
                {
                    throw new PagerNotFoundException(group.Key);
                }

                pager.FreePage(group.Select(i => i.PageNumber));
            }
        }
    }
}
