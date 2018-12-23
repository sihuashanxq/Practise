using System;
using System.Collections.Generic;
using Vicuna.Storage.Pages;
using Vicuna.Storage.Slices;

namespace Vicuna.Storage
{
    public unsafe class StorageSliceUsageNode
    {
        private readonly byte[] _pageContent;

        public StorageSliceUsageNode(byte[] pageContent)
        {
            _pageContent = pageContent;
        }

        public PageHeader PageHeader
        {
            get
            {
                fixed (byte* buffer = _pageContent)
                {
                    return *(PageHeader*)buffer;
                }
            }
        }

        public long PageOffset
        {
            get
            {
                fixed (byte* buffer = _pageContent)
                {
                    return ((PageHeader*)buffer)->PagePos;
                }
            }
            set
            {
                fixed (byte* buffer = _pageContent)
                {
                    ((PageHeader*)buffer)->PagePos = value;
                }
            }
        }

        public long PrePageOffset
        {
            get
            {
                fixed (byte* buffer = _pageContent)
                {
                    return ((PageHeader*)buffer)->PrePagePos;
                }
            }
            set
            {
                fixed (byte* buffer = _pageContent)
                {
                    ((PageHeader*)buffer)->PrePagePos = value;
                }
            }
        }

        public long NextPageOffset
        {
            get
            {
                fixed (byte* buffer = _pageContent)
                {
                    return ((PageHeader*)buffer)->NextPagePos;
                }
            }
            set
            {
                fixed (byte* buffer = _pageContent)
                {
                    ((PageHeader*)buffer)->NextPagePos = value;
                }
            }
        }

        public short Count
        {
            get
            {
                fixed (byte* buffer = _pageContent)
                {
                    return ((PageHeader*)buffer)->ItemCount;
                }
            }
        }

        public StorageSliceSpaceEntry FirstEntry
        {
            get
            {
                fixed (byte* buffer = _pageContent)
                {
                    var pageHead = (PageHeader*)buffer;
                    var entryPointer = (StorageSliceSpaceUsage*)buffer[Constants.PageHeaderSize];

                    return new StorageSliceSpaceEntry()
                    {
                        Index = 0,
                        Usage = *entryPointer,
                        OwnerOffset = pageHead->PagePos
                    };
                }
            }
        }

        public StorageSliceSpaceEntry LastEntry
        {
            get
            {
                fixed (byte* buffer = _pageContent)
                {
                    var pageHead = (PageHeader*)buffer;
                    var entryPointer = (StorageSliceSpaceUsage*)buffer[Constants.PageHeaderSize];

                    return new StorageSliceSpaceEntry()
                    {
                        Index = pageHead->ItemCount - 1,
                        Usage = entryPointer[pageHead->ItemCount - 1],
                        OwnerOffset = pageHead->PagePos
                    };
                }
            }
        }

        public bool IsFull
        {
            get
            {
                fixed (byte* buffer = _pageContent)
                {
                    return ((PageHeader*)buffer)->ItemCount == 1024;
                }
            }
        }

        public bool IsEmpty
        {
            get
            {
                fixed (byte* buffer = _pageContent)
                {
                    return ((PageHeader*)buffer)->ItemCount == 0;
                }
            }
        }

        public void Delete(int index)
        {
            if (index < 0 || index >= Count)
            {
                throw new IndexOutOfRangeException(nameof(index));
            }

            fixed (byte* buffer = _pageContent)
            {
                var count = Count;
                var pageHead = (PageHeader*)buffer;
                var entryPointer = (StorageSliceSpaceUsage*)&buffer[Constants.PageHeaderSize];

                for (var i = index; i < count - 1; i++)
                {
                    entryPointer[i] = entryPointer[i + 1];
                }

                entryPointer[Count - 1].PageOffset = -1;
                entryPointer[Count - 1].UsedLength = 0;
                pageHead->ItemCount--;
            }
        }

        public void Insert(StorageSliceSpaceUsage usage)
        {
            fixed (byte* buffer = _pageContent)
            {
                var index = 0;
                var pageHead = (PageHeader*)buffer;
                var entryPointer = (StorageSliceSpaceUsage*)&buffer[Constants.PageHeaderSize];

                for (; index < pageHead->ItemCount; index++)
                {
                    if (entryPointer[index].UsedLength >= usage.UsedLength)
                    {
                        break;
                    }
                }

                for (var i = pageHead->ItemCount; i > index; i--)
                {
                    entryPointer[i] = entryPointer[i - 1];
                }

                entryPointer[index] = usage;
                pageHead->ItemCount++;
            }
        }

        public void Update(StorageSliceSpaceEntry entry)
        {
            fixed (byte* buffer = _pageContent)
            {
                var pageHead = (PageHeader*)buffer;
                var entryPointer = (StorageSliceSpaceUsage*)&buffer[Constants.PageHeaderSize];
                if (entryPointer[entry.Index].UsedLength > entry.Usage.UsedLength)
                {
                    entryPointer[entry.Index] = entry.Usage;
                    MoveUpdateEntryIncrement(entryPointer, 0, entry.Index);
                }
                else
                {
                    entryPointer[entry.Index] = entry.Usage;
                    MoveUpdateEntryDecrement(entryPointer, entry.Index, pageHead->ItemCount);
                }
            }
        }

        public void MoveUpdateEntryIncrement(StorageSliceSpaceUsage* entryPointer, int changedLeft, int changedRight)
        {
            for (var i = changedRight; i >= changedLeft + 1; i--)
            {
                if (entryPointer[i].UsedLength > entryPointer[i - 1].UsedLength)
                {
                    break;
                }

                var tmp = entryPointer[i];
                entryPointer[i] = entryPointer[i - 1];
                entryPointer[i - 1] = tmp;
            }
        }

        public void MoveUpdateEntryDecrement(StorageSliceSpaceUsage* entryPointer, int changedLeft, int changedRight)
        {
            for (var i = changedLeft; i < changedRight - 1; i++)
            {
                if (entryPointer[i].UsedLength >= entryPointer[i + 1].UsedLength)
                {
                    break;
                }

                var tmp = entryPointer[i];
                entryPointer[i] = entryPointer[i + 1];
                entryPointer[i + 1] = tmp;
            }
        }

        public List<StorageSliceSpaceUsage> GetEntries()
        {
            var entries = new List<StorageSliceSpaceUsage>();

            fixed (byte* buffer = _pageContent)
            {
                var pageHead = (PageHeader*)buffer;
                var entryPointer = (StorageSliceSpaceUsage*)&buffer[Constants.PageHeaderSize];
                for (var i = 0; i < pageHead->ItemCount; i++)
                {
                    entries.Add(entryPointer[i]);
                }
            }

            return entries;
        }
    }
}
