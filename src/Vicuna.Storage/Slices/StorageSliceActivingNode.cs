using System;
using System.Collections.Generic;
using Vicuna.Storage.Pages;
using Vicuna.Storage.Slices;

namespace Vicuna.Storage
{
    public unsafe class StorageSliceActivingNode
    {
        internal Page NodePage { get; }

        public StorageSliceActivingNode(Page nodePage)
        {
            NodePage = nodePage;
        }

        public bool IsFull => NodePage.ItemCount == 1024;

        public bool IsEmpty => NodePage.ItemCount == 0;

        public long PageOffset
        {
            get => NodePage.PageOffset;
            set => NodePage.PageOffset = value;
        }

        public long PrePageOffset
        {
            get => NodePage.PrePageOffset;
            set => NodePage.PrePageOffset = value;
        }

        public long NextPageOffset
        {
            get => NodePage.NextPageOffset;
            set => NodePage.NextPageOffset = value;
        }

        public StorageSliceSpaceEntry FirstEntry
        {
            get
            {
                fixed (byte* buffer = NodePage.Buffer)
                {
                    var pageHead = (PageHeader*)buffer;
                    var entryPointer = (SpaceUsage*)&buffer[Constants.PageHeaderSize];

                    return new StorageSliceSpaceEntry()
                    {
                        Index = 0,
                        Usage = *entryPointer,
                        OwnerOffset = pageHead->PageOffset
                    };
                }
            }
        }

        public StorageSliceSpaceEntry LastEntry
        {
            get
            {
                fixed (byte* buffer = NodePage.Buffer)
                {
                    var pageHead = (PageHeader*)buffer;
                    var entryPointer = (SpaceUsage*)&buffer[Constants.PageHeaderSize];

                    return new StorageSliceSpaceEntry()
                    {
                        Index = pageHead->ItemCount - 1,
                        Usage = entryPointer[pageHead->ItemCount - 1],
                        OwnerOffset = pageHead->PageOffset
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
                var entryPointer = (SpaceUsage*)&buffer[Constants.PageHeaderSize];

                for (var i = index; i < count - 1; i++)
                {
                    entryPointer[i] = entryPointer[i + 1];
                }

                entryPointer[count - 1].PageOffset = -1;
                entryPointer[count - 1].UsedLength = 0;
                pageHead->ItemCount--;
            }
        }

        public int Insert(SpaceUsage usage)
        {
            fixed (byte* buffer = NodePage.Buffer)
            {
                var index = 0;
                var pageHead = (PageHeader*)buffer;
                var entryPointer = (SpaceUsage*)&buffer[Constants.PageHeaderSize];

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

        public void Update(StorageSliceSpaceEntry entry)
        {
            fixed (byte* buffer = NodePage.Buffer)
            {
                var pageHead = (PageHeader*)buffer;
                var entryPointer = (SpaceUsage*)&buffer[Constants.PageHeaderSize];
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

        public void MoveUpdateEntryIncrement(SpaceUsage* entryPointer, int changedLeft, int changedRight)
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

        public void MoveUpdateEntryDecrement(SpaceUsage* entryPointer, int changedLeft, int changedRight)
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

        public List<SpaceUsage> GetEntries()
        {
            var entries = new List<SpaceUsage>();

            fixed (byte* buffer = NodePage.Buffer)
            {
                var pageHead = (PageHeader*)buffer;
                var entryPointer = (SpaceUsage*)&buffer[Constants.PageHeaderSize];
                for (var i = 0; i < pageHead->ItemCount; i++)
                {
                    entries.Add(entryPointer[i]);
                }
            }

            return entries;
        }
    }
}
