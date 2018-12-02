using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Vicuna.Storage.Pages;

namespace Vicuna.Storage
{
    public class StorageSegment
    {
        private StorageSlice _activeSlice;

        private readonly Stack<long> _freeSlices;

        private readonly HashSet<long> _fullSlices;

        private readonly ConcurrentDictionary<long, StorageSliceSpaceEntry> _notFullSlices;

        public long Loc { get; set; }

        public StorageSlice ActiveSlice
        {
            get
            {
                if (_activeSlice == null)
                {
                    _activeSlice = new StorageSlice(-1);
                }

                return _activeSlice;
            }
        }

        internal StorageSegmentSpaceEntry SpaceEntry { get; }

        public StorageSegment(AllocatedPageBuffer page)
        {
            _freeSlices = new Stack<long>();
            _fullSlices = new HashSet<long>();
            _notFullSlices = new ConcurrentDictionary<long, StorageSliceSpaceEntry>();
        }

        public bool Allocate(int size, out long loc)
        {
            if (Allocate(ActiveSlice, size, out loc))
            {
                return true;
            }

            foreach (var spaceEntry in _notFullSlices.Values.ToList())
            {
                if (spaceEntry.UsedSize + size > 1024 * 1024)
                {
                    continue;
                }

                var slice = null as StorageSlice;
                if (slice == null)
                {
                    continue;
                }

                if (Allocate(slice, size, out loc))
                {
                    return true;
                }
            }

            var newSlice = AllocateSlice();
            if (newSlice == null)
            {
                loc = -1;
                return false;
            }

            return Allocate(newSlice, size, out loc);
        }

        private bool Allocate(StorageSlice slice, int size, out long loc)
        {
            if (slice == null)
            {
                loc = -1;
                return false;
            }


            if (!slice.Allocate(size, out loc))
            {
                return false;
            }

            if (slice.SpaceEntry.UsedSize == StorageSliceSpaceEntry.Capacity)
            {
                _fullSlices.Add(slice.Loc);
                _notFullSlices.TryRemove(slice.Loc, out var _);
            }
            else
            {
                _notFullSlices[slice.Loc] = slice.SpaceEntry;
            }

            _activeSlice = slice;
            return true;
        }

        public List<AllocatedPageBuffer> AllocatePage(int count)
        {
            if (count > 1024)
            {
                throw new InvalidOperationException($"allocated page count:{count} more than 1024 at once!");
            }

            return null;
        }

        private StorageSlice AllocateSlice()
        {
            return null;
        }

        internal IEnumerable<StorageSliceSpaceEntry> GetSlices()
        {
            foreach (var item in _fullSlices)
            {
                yield return new StorageSliceSpaceEntry(item, 0);
            }

            foreach (var item in _freeSlices)
            {
                yield return new StorageSliceSpaceEntry(item);
            }

            foreach (var item in _notFullSlices)
            {
                yield return item.Value;
            }
        }
    }

    internal class StorageSegmentSpaceEntry
    {
        public const long Capacity = 1024L * 1024L * 16L * 512L;

        public long Loc { get; internal set; }

        public long UsedSize { get; internal set; }

        public StorageSegmentSpaceEntry(long loc) : this(loc, 0)
        {

        }

        public StorageSegmentSpaceEntry(long loc, long usedSize)
        {
            Loc = loc;
            UsedSize = usedSize;
        }
    }
}
