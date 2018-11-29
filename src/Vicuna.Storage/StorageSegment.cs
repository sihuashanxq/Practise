using Vicuna.Storage.Pages;

namespace Vicuna.Storage
{
    public class StorageSegment
    {
        public long Loc { get; set; }

        public StorageSegmentSpaceEntry SpaceEntry { get; }

        public StorageSegment(Page page)
        {

        }

        public StorageSegment(Page page, StorageSegmentSpaceEntry spaceEntry)
        {
            SpaceEntry = spaceEntry;
        }

        public bool Allocate(int size, out long loc)
        {
            loc = -1;
            return false;
        }

        public Page Allocate()
        {
            return Allocate(1)[0];
        }

        public Page[] Allocate(int count)
        {
            return null;
        }
    }
}
