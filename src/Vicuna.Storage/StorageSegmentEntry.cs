namespace Vicuna.Storage
{
    public class StorageSegmentEntry
    {
        public long Loc { get; internal set; }

        public long Used { get; internal set; }

        public StorageSegmentEntry(long loc) : this(loc, 0)
        {
        }

        public StorageSegmentEntry(long loc, long used)
        {
            Loc = loc;
            Used = used;
        }
    }
}
