namespace Vicuna.Storage
{
    public class StorageSlicePageEntry
    {
        public short Offset { get; internal set; }

        public short Used { get; internal set; }

        public StorageSlicePageEntry(short offset) 
            : this(offset, 0)
        {
        }

        public StorageSlicePageEntry(short offset, short used)
        {
            Offset = offset;
            Used = used;
        }
    }
}
