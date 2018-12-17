using System;
using Vicuna.Storage.Pages;

namespace Vicuna.Storage
{
    public class StoragePage : Page
    {
        public StoragePage(byte[] buffer) : base(buffer)
        {

        }

        public virtual unsafe StorageSpaceEntry GetEntry(int offset)
        {
            if (offset > Buffer.Length || offset < 0)
            {
                throw new IndexOutOfRangeException(nameof(offset));
            }

            fixed (byte* buffer = &Buffer[offset])
            {
                return new StorageSpaceEntry(*(long*)buffer, *(long*)(buffer + sizeof(long)));
            }
        }

        public virtual unsafe void SetEntry(int offset, StorageSpaceEntry entry)
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

        public StorageSpaceEntry GetFirstEntry() => GetEntry(Constants.PageHeaderSize);
    }
}
