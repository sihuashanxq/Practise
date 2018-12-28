using System;
using System.Collections.Generic;
using Vicuna.Storage.Pages;
using Vicuna.Storage.Slices;

namespace Vicuna.Storage
{
    public unsafe class StorageSliceUsageNode
    {
        private readonly Page _nodePage;

        public StorageSliceUsageNode(Page nodePage)
        {
            _nodePage = nodePage;
        }

        public short Count => _nodePage.GetItemCount();

        public bool IsFull => Count == 1024;

        public bool IsEmpty => Count == 0;

        public long PageOffset
        {
            get => _nodePage.PageOffset;
            set => _nodePage.PageOffset = value;
        }

        public long PrePageOffset
        {
            get => _nodePage.PrePageOffset;
            set => _nodePage.PrePageOffset = value;
        }

        public long NextPageOffset
        {
            get => _nodePage.NextPageOffset;
            set => _nodePage.NextPageOffset = value;
        }

        public StorageSliceSpaceEntry FirstEntry
        {
            get
            {
                fixed (byte* buffer = _nodePage.Buffer)
                {
                    var pageHead = (PageHeader*)buffer;
                    var entryPointer = (StorageSliceSpaceUsage*)&buffer[Constants.PageHeaderSize];

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
                fixed (byte* buffer = _nodePage.Buffer)
                {
                    var pageHead = (PageHeader*)buffer;
                    var entryPointer = (StorageSliceSpaceUsage*)&buffer[Constants.PageHeaderSize];

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
            if (index < 0 || index >= Count)
            {
                throw new IndexOutOfRangeException(nameof(index));
            }

            fixed (byte* buffer = _nodePage.Buffer)
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
            fixed (byte* buffer = _nodePage.Buffer)
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
            fixed (byte* buffer = _nodePage.Buffer)
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

        public void InitializeNodePage()
        {
            fixed (byte* buffer = _nodePage.Buffer)
            {
                var pageHead = (PageHeader*)buffer;

                pageHead->ItemCount = 0;
                pageHead->ModifiedCount = 0;
                pageHead->FreeSize = 0;
            }
        }

        public List<StorageSliceSpaceUsage> GetEntries()
        {
            var entries = new List<StorageSliceSpaceUsage>();

            fixed (byte* buffer = _nodePage.Buffer)
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
