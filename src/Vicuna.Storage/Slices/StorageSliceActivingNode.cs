using System;
using System.Collections.Generic;
using Vicuna.Storage.Pages;
using Vicuna.Storage.Slices;

namespace Vicuna.Storage
{
    public unsafe class StorageSliceActivingNode
    {
        internal Page NodePage { get; }

        internal const int Capacity = 1024;

        public StorageSliceActivingNode(Page nodePage)
        {
            NodePage = nodePage;
        }

        public bool IsFull => NodePage.ItemCount == Capacity;

        public bool IsEmpty => NodePage.ItemCount == 0;

        public long PageNumber
        {
            get => NodePage.PageNumber;
            set => NodePage.PageNumber = value;
        }

        public long PrePageNumber
        {
            get => NodePage.PrePageNumber;
            set => NodePage.PrePageNumber = value;
        }

        public long NextPageNumber
        {
            get => NodePage.NextPageNumber;
            set => NodePage.NextPageNumber = value;
        }

        public StorageSliceUsageEntry FirstEntry
        {
            get
            {
                fixed (byte* buffer = NodePage.Buffer)
                {
                    var pageHead = (PageHeader*)buffer;
                    var entryPointer = (StorageSliceUsage*)&buffer[Constants.PageHeaderSize];

                    return new StorageSliceUsageEntry()
                    {
                        OwnerIndex = 0,
                        Usage = *entryPointer,
                        OwnerPageNumber = pageHead->PageNumber
                    };
                }
            }
        }

        public StorageSliceUsageEntry LastEntry
        {
            get
            {
                fixed (byte* buffer = NodePage.Buffer)
                {
                    var pageHead = (PageHeader*)buffer;
                    var entryPointer = (StorageSliceUsage*)&buffer[Constants.PageHeaderSize];

                    return new StorageSliceUsageEntry()
                    {
                        OwnerIndex = pageHead->ItemCount - 1,
                        Usage = entryPointer[pageHead->ItemCount - 1],
                        OwnerPageNumber = pageHead->PageNumber
                    };
                }
            }
        }

        public void Delete(int index)
        {
            if (index < 0 || index >= NodePage.ItemCount)
            {
                throw new IndexOutOfRangeException(nameof(index));
            }

            fixed (byte* buffer = NodePage.Buffer)
            {
                var count = NodePage.ItemCount;
                var pageHead = (PageHeader*)buffer;
                var entryPointer = (StorageSliceUsage*)&buffer[Constants.PageHeaderSize];

                for (var i = index; i < count - 1; i++)
                {
                    entryPointer[i] = entryPointer[i + 1];
                }

                entryPointer[count - 1].PageNumber = -1;
                entryPointer[count - 1].UsedLength = -1;
                pageHead->ItemCount--;
            }
        }

        public int Insert(StorageSliceUsage usage)
        {
            fixed (byte* buffer = NodePage.Buffer)
            {
                var index = 0;
                var pageHead = (PageHeader*)buffer;
                var entryPointer = (StorageSliceUsage*)&buffer[Constants.PageHeaderSize];

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

                return index;
            }
        }

        public void Update(StorageSliceUsageEntry entry)
        {
            fixed (byte* buffer = NodePage.Buffer)
            {
                var pageHead = (PageHeader*)buffer;
                var entryPointer = (StorageSliceUsage*)&buffer[Constants.PageHeaderSize];
                if (entryPointer[entry.OwnerIndex].UsedLength > entry.Usage.UsedLength)
                {
                    entryPointer[entry.OwnerIndex] = entry.Usage;
                    MoveUpdateEntryIncrement(entryPointer, 0, entry.OwnerIndex);
                }
                else
                {
                    entryPointer[entry.OwnerIndex] = entry.Usage;
                    MoveUpdateEntryDecrement(entryPointer, entry.OwnerIndex, pageHead->ItemCount);
                }
            }
        }

        public void MoveUpdateEntryIncrement(StorageSliceUsage* entryPointer, int changedLeft, int changedRight)
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

        public void MoveUpdateEntryDecrement(StorageSliceUsage* entryPointer, int changedLeft, int changedRight)
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

        public List<StorageSliceUsage> GetEntries()
        {
            var entries = new List<StorageSliceUsage>();

            fixed (byte* buffer = NodePage.Buffer)
            {
                var pageHead = (PageHeader*)buffer;
                var entryPointer = (StorageSliceUsage*)&buffer[Constants.PageHeaderSize];
                for (var i = 0; i < pageHead->ItemCount; i++)
                {
                    entries.Add(entryPointer[i]);
                }
            }

            return entries;
        }
    }
}
