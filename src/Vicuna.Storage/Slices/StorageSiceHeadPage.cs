using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Vicuna.Storage.Pages;
using Vicuna.Storage.Slices;

namespace Vicuna.Storage
{
    public unsafe class StorageSiceHeadPage : Page
    {
        const int SlicePageCount = 1024;

        public int FreePageCount => FreePageIndexs.Count;

        public int FullPageCount => FullPageIndexs.Count;

        public int ActivedNodeIndex
        {
            get => this.GetAcitvedNodeIndex();
            set => this.SetActivedNodeIndex(value);
        }

        public long ActivedNodeOffset
        {
            get => this.GetAcitvedNodeOffset();
            set => this.SetActivedNodeOffset(value);
        }

        public HashSet<int> FreePageIndexs { get; }

        public HashSet<int> FullPageIndexs { get; }

        public ConcurrentDictionary<int, SpaceUsage> ActivedPageMapping { get; }

        public StorageSiceHeadPage(byte[] buffer) : base(buffer)
        {
            FreePageIndexs = new HashSet<int>();
            FullPageIndexs = new HashSet<int>();
            ActivedPageMapping = new ConcurrentDictionary<int, SpaceUsage>();
            BuildPageEntries();
        }

        public void SetPageEntry(int index, int oldUsedLength, int newUsedLength)
        {
            SetPageEntry(index, oldUsedLength, new SpaceUsage(PageOffset + index, newUsedLength));
        }

        public void SetPageEntry(int oldUsedLength, SlicePageUsageEntry pageEntry)
        {
            SetPageEntry(pageEntry.Index, oldUsedLength, pageEntry.Usage);
        }

        public void SetPageEntry(int index, int oldUsedLength, SpaceUsage usage)
        {
            if (usage.UsedLength >= Constants.PageSize - 128)
            {
                usage.UsedLength = Constants.PageSize;
                SetPageEntryIsFull(index, oldUsedLength);
            }
            else if (usage.UsedLength <= Constants.PageHeaderSize)
            {
                usage.UsedLength = Constants.PageHeaderSize;
                SetPageEntryIsFree(index, oldUsedLength);
            }
            else
            {
                SetPageEntryIsActived(index, oldUsedLength, usage);
            }

            fixed (byte* buffer = Buffer)
            {
                var pageHeader = (PageHeader*)buffer;
                var pagePointer = (SpaceUsage*)&buffer[Constants.PageHeaderSize];

                pagePointer[index] = usage;
                pageHeader->ModifiedCount++;
                pageHeader->UsedLength = pageHeader->UsedLength + usage.UsedLength - oldUsedLength;
            }
        }

        public SlicePageUsageEntry GetPageEntry(int index)
        {
            if (index < 0 || index > SlicePageCount)
            {
                throw new IndexOutOfRangeException(nameof(index));
            }

            fixed (byte* buffer = Buffer)
            {
                return GetPageEntry((SpaceUsage*)&buffer[Constants.PageHeaderSize], index);
            }
        }

        public List<SlicePageUsageEntry> GetPageEntries()
        {
            var pageUsagEntries = new List<SlicePageUsageEntry>();

            fixed (byte* buffer = Buffer)
            {
                var pointer = (SpaceUsage*)&buffer[Constants.PageHeaderSize];

                for (var index = 0; index < SlicePageCount; index++)
                {
                    pageUsagEntries.Add(GetPageEntry(pointer, index));
                }
            }

            return pageUsagEntries;
        }

        private SlicePageUsageEntry GetPageEntry(SpaceUsage* pointer, int index)
        {
            if (index < 0 || index > SlicePageCount)
            {
                throw new IndexOutOfRangeException(nameof(index));
            }

            var usage = pointer[index];
            if (usage.UsedLength == 0)
            {
                usage.UsedLength = (int)Constants.PageHeaderSize;
            }

            if (usage.PageOffset != PageOffset + index)
            {
                usage.PageOffset = PageOffset + index;
            }

            return new SlicePageUsageEntry(index, usage);
        }

        private void SetPageEntryIsFull(int index, int oldUsedLength)
        {
            if (oldUsedLength <= Constants.PageHeaderSize)
            {
                FreePageIndexs.Remove(index);
                FullPageIndexs.Add(index);
                return;
            }

            if (oldUsedLength < Constants.PageSize - 128)
            {
                ActivedPageMapping.TryRemove(index, out var _);
                FullPageIndexs.Add(index);
            }
        }

        private void SetPageEntryIsFree(int index, int oldUsedLength)
        {
            if (oldUsedLength >= Constants.PageSize - 128)
            {
                FullPageIndexs.Remove(index);
                FreePageIndexs.Add(index);
                return;
            }

            if (oldUsedLength > Constants.PageHeaderSize)
            {
                ActivedPageMapping.TryRemove(index, out var _);
                FreePageIndexs.Add(index);
            }
        }

        private void SetPageEntryIsActived(int index, int oldUsedLength, SpaceUsage usage)
        {
            if (oldUsedLength >= Constants.PageSize - 128)
            {
                FullPageIndexs.Remove(index);
                ActivedPageMapping.AddOrUpdate(index, usage, (k, v) => usage);
                return;
            }

            if (oldUsedLength <= Constants.PageHeaderSize)
            {
                FreePageIndexs.Remove(index);
                ActivedPageMapping.AddOrUpdate(index, usage, (k, v) => usage);
            }
        }

        private void BuildPageEntries()
        {
            fixed (byte* buffer = Buffer)
            {
                var pageEntryPointer = (SpaceUsage*)&buffer[Constants.PageHeaderSize];

                for (var index = 0; index < SlicePageCount; index++)
                {
                    var entry = GetPageEntry(pageEntryPointer, index);
                    if (entry.Usage.UsedLength <= Constants.PageHeaderSize)
                    {
                        FreePageIndexs.Add(entry.Index);
                        continue;
                    }

                    if (entry.Usage.UsedLength >= Constants.PageSize - 128)
                    {
                        FullPageIndexs.Add(entry.Index);
                        continue;
                    }

                    ActivedPageMapping[index] = entry.Usage;
                }
            }
        }
    }
}
