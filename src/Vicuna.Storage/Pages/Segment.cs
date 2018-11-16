using System.Collections.Generic;

namespace Vicuna.Storage.Pages
{
    public class Segment
    {
        protected SegmentHeadPage Page { get; }

        protected SortedSet<SegmentHeadPage.AllocationEntry> AllocationEntries => Page.AllocationEntries;

        public Segment(long pageId)
        {
            Page = new SegmentHeadPage(null) { PageId = pageId };
        }

        protected Segment(SegmentHeadPage page)
        {
            Page = page;
        }

        public bool GetHasFreeSpacePageToAllocate(int size, out long pageid)
        {
            foreach (var entry in AllocationEntries)
            {
                if (entry.MaxFreeSize >= size)
                {
                    pageid = Page.PageId + entry.PageOffset;
                    return true;
                }

                break;
            }

            pageid = -1;
            return false;
        }
    }
}
