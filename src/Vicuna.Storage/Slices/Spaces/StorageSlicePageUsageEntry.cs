namespace Vicuna.Storage.Slices
{
    public class StorageSlicePageUsageEntry
    {
        public int Index { get; set; }

        public StorageSlicePageUsage Usage { get; set; }

        public StorageSlicePageUsageEntry(int index, StorageSlicePageUsage usage)
        {
            Index = index;
            Usage = usage;
        }
    }
}
