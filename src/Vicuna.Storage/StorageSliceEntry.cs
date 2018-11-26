namespace Vicuna.Storage
{
    public class StorageSliceEntry
    {
        public long Loc { get; internal set; }

        public int Used { get; internal set; }

        public StorageSliceEntry(long loc) : this(loc, 0)
        {
        }

        public StorageSliceEntry(long loc, int used)
        {
            Loc = loc;
            Used = used;
        }
    }
}
