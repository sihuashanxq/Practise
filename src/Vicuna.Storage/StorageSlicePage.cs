using System;

namespace Vicuna.Storage
{
    public class StorageSlicePage : StoragePage
    {
        public StorageSlicePage(byte[] buffer) : base(buffer)
        {

        }

        public override unsafe StorageSpaceEntry GetEntry(int offset)
        {
            if (offset > Buffer.Length || offset < 0)
            {
                throw new IndexOutOfRangeException(nameof(offset));
            }

            fixed (byte* buffer = &Buffer[offset])
            {
                return new StorageSpaceEntry(*(long*)buffer, *(short*)(buffer + sizeof(long)));
            }
        }

        public override unsafe void SetEntry(int offset, StorageSpaceEntry entry)
        {
            if (offset > Buffer.Length || offset < 0)
            {
                throw new IndexOutOfRangeException(nameof(offset));
            }
      
            fixed (byte* buffer = &Buffer[offset])
            {
                *(long*)buffer = entry.Pos;
                *(short*)(buffer + sizeof(long)) = (short)entry.UsedSize;
            }
        }
    }
}
