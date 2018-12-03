namespace Vicuna.Storage
{
    public struct StorageSpaceUsageEntry
    {
        public long Pos;

        public long UsedSize;

        public StorageSpaceUsageEntry(long pos)
             : this(pos, 0)
        {

        }

        public StorageSpaceUsageEntry(long pos, long usedSize)
        {
            Pos = pos;
            UsedSize = usedSize;
        }
    }

}
