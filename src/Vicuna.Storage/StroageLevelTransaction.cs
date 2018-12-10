using System.Collections.Concurrent;
using System.Linq;
using Vicuna.Storage.Pages;

namespace Vicuna.Storage
{
    public class StroageLevelTransaction
    {
        private readonly ConcurrentDictionary<long, Page> _cachedPages;

        private readonly ConcurrentDictionary<long, Page> _modifiedPages;

        private readonly ConcurrentDictionary<long, Page> _allocatedPages;

        public StroageLevelTransaction()
        {
            _cachedPages = new ConcurrentDictionary<long, Page>();
            _modifiedPages = new ConcurrentDictionary<long, Page>();
            _allocatedPages = new ConcurrentDictionary<long, Page>();
        }

        public Page GetPage(long pos)
        {
            if (_modifiedPages.TryGetValue(pos, out var modifiedPage))
            {
                return modifiedPage;
            }

            if (_allocatedPages.TryGetValue(pos, out var allcoatedPage))
            {
                return allcoatedPage;
            }

            return null;
            //return _cachedPages.GetOrAdd(pos, k => _snapshotPager.GetPage(pos));
        }

        public Page GetPageToModify(long pos)
        {
            return _modifiedPages.GetOrAdd(pos, k => GetPage(pos));
        }

        public bool AllocatePage(out Page page)
        {
            if (!AllocatePage(1, out var pages))
            {
                page = null;
                return false;
            }

            page = pages.First();
            return true;
        }

        public bool AllocatePage(int pageCount, out Page[] pages)
        {
            pages = null;
            //if (!LastUsedSegment.AllocatePage(pageCount, out pages))
            //{
            //    pages = null;
            //    return false;
            //}

            //foreach (var page in pages)
            //{
            //    _allocatedPages.TryAdd(page.PagePos, page);
            //}

            return true;
        }
    }
}
