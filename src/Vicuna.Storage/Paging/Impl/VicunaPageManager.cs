using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Vicuna.Storage.Paging.Impl
{
    public class VicunaPageManager : IPageManager
    {
        private readonly ConcurrentDictionary<int, IPager> _pagerMaps;

        public VicunaPageManager()
        {
            _pagerMaps = new ConcurrentDictionary<int, IPager>();
        }

        public PageIdentity Allocate(int token)
        {
            return Allocate(token, 1)[0];
        }

        public PageIdentity[] Allocate(int token, uint count)
        {
            if (!_pagerMaps.TryGetValue(token, out var pager) || pager == null)
            {
                throw new PagerNotFoundException(token);
            }

            return pager.Allocator.Allocate(count);
        }

        public void SetPage(PageIdentity identity, Page page)
        {
            if (!_pagerMaps.TryGetValue(identity.Token, out var pager) || pager == null)
            {
                throw new PagerNotFoundException(identity.Token);
            }

            pager.SetPageData(identity.PageNumber, page.Data);
        }

        public Page GetPage(PageIdentity identity)
        {
            if (!_pagerMaps.TryGetValue(identity.Token, out var pager) || pager == null)
            {
                throw new PagerNotFoundException(identity.Token);
            }

            return new Page(pager.GetPageData(identity.PageNumber));
        }

        public void Free(IEnumerable<PageIdentity> pages)
        {
            foreach (var group in pages.GroupBy(i => i.Token))
            {
                if (!_pagerMaps.TryGetValue(group.Key, out var pager) || pager == null)
                {
                    throw new PagerNotFoundException(group.Key);
                }

                pager.Allocator.Free(group);
            }
        }
    }

    public class PagerNotFoundException : Exception
    {
        public int Token { get; }

        public PagerNotFoundException(int token) : base($"can't find a pager ref then token{token} ")
        {
            Token = token;
        }
    }
}
