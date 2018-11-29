namespace Vicuna.Storage
{
    public class StorageSliceSpaceEntry
    {
        public long Loc { get; internal set; }

        public int UsedSize { get; internal set; }

        public StorageSliceSpaceEntry(long loc) : this(loc, 0)
        {
        }

        public StorageSliceSpaceEntry(long loc, int usedSize)
        {
            Loc = loc;
            UsedSize = usedSize;
        }
    }
}
