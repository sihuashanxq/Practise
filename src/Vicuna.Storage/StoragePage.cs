using System;
using Vicuna.Storage.Pages;

namespace Vicuna.Storage
{
    public class StoragePage : Page
    {
        public StoragePage(byte[] buffer) : base(buffer)
        {
        }

        public virtual unsafe StorageSpaceUsageEntry GetEntry(int offset)
        {
            if (offset > Buffer.Length || offset < 0)
            {
                throw new IndexOutOfRangeException(nameof(offset));
            }

            fixed (byte* buffer = &Buffer[offset])
            {
                return new StorageSpaceUsageEntry(*(long*)buffer, *(long*)(buffer + sizeof(long)));
            }
        }

        public virtual unsafe void AddEntry(int offset, StorageSpaceUsageEntry entry)
        {
            if (offset > Buffer.Length || offset < 0)
            {
                throw new IndexOutOfRangeException(nameof(offset));
            }

            fixed (byte* buffer = &Buffer[offset])
            {
                *(long*)buffer = entry.Pos;
                *(long*)(buffer + sizeof(long)) = entry.UsedSize;
            }
        }
    }
}
