using System;

namespace Vicuna.Storage.Pages
{
    public class Segment
    {
        private readonly SegmentPage _head;

        private readonly PageManager _pageManager;

        protected long HeadPageId => _head.PageId;

        protected SegmentPage.AllocationEntry[] AllocationEntries => _head.AllocationEntries;

        public Segment(long headPageId, PageManager pageManager)
        {
            _pageManager = pageManager;
        }

        protected Segment(SegmentPage page, PageManager pageManager)
        {
            _pageManager = pageManager;
        }

        public bool GetHasFreeSpacePage(int size, out long id)
        {
            for (var i = 1; i < AllocationEntries.Length; i++)
            {
                if (AllocationEntries[i].MaxFreeSize >= size)
                {
                    id = HeadPageId + i;
                    return true;
                }
            }

            id = -1;
            return false;
        }
    }
}
