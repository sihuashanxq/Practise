namespace Vicuna.Storage
{
    public class StorageSegmentSpaceEntry
    {
        public const long Capacity = 1024L * 1024L * 16L * 512L;

        public long Loc { get; internal set; }

        public long UsedSize { get; internal set; }

        public StorageSegmentSpaceEntry(long loc) : this(loc, 0)
        {

        }

        public StorageSegmentSpaceEntry(long loc, long usedSize)
        {
            Loc = loc;
            UsedSize = usedSize;
        }
    }
}
