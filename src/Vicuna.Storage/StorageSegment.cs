using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Vicuna.Storage
{
    public class StorageSegment
    {
        private StorageSlice _lastUsedSlice;

        private readonly Stack<long> _freeSlices;

        private readonly HashSet<long> _fullSlices;

        private readonly ConcurrentDictionary<long, StorageSpaceUsageEntry> _notFullSlices;

        public long Loc { get; set; }

        public StorageSlice LastUsedSlice
        {
            get
            {
                if (_lastUsedSlice == null)
                {
                    _lastUsedSlice = new StorageSlice(null, null);
                }

                return _lastUsedSlice;
            }
        }

        public StorageSpaceUsageEntry Usage { get; }

        public StorageSegment()
        {
            _freeSlices = new Stack<long>();
            _fullSlices = new HashSet<long>();
            _notFullSlices = new ConcurrentDictionary<long, StorageSpaceUsageEntry>();
        }

        public bool Allocate(int size, out AllocationBuffer buffer)
        {
            if (Allocate(LastUsedSlice, size, out buffer))
            {
                return true;
            }

            foreach (var entry in _notFullSlices.Values.ToList())
            {
                if (entry.UsedSize + size > Constants.StorageSliceSize)
                {
                    continue;
                }

                var slice = null as StorageSlice;
                if (slice == null)
                {
                    continue;
                }

                if (Allocate(slice, size, out buffer))
                {
                    return true;
                }
            }

            return Allocate(AllocateSlice(), size, out buffer);
        }

        private bool Allocate(StorageSlice slice, int size, out AllocationBuffer buffer)
        {
            if (slice == null)
            {
                buffer = null;
                return false;
            }

            if (!slice.Allocate(size, out buffer))
            {
                return false;
            }

            if (slice.Usage.UsedSize == Constants.StorageSliceSize)
            {
                _lastUsedSlice = null;
                _fullSlices.Add(slice.Loc);
                _notFullSlices.TryRemove(slice.Loc, out var _);
            }
            else
            {
                _lastUsedSlice = slice;
                _notFullSlices[slice.Loc] = slice.Usage;
            }

            return true;
        }

        public AllocationBuffer[] AllocatePage(int count)
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

        internal IEnumerable<StorageSpaceUsageEntry> GetSlices()
        {
            foreach (var item in _fullSlices)
            {
                yield return new StorageSpaceUsageEntry(item, 0);
            }

            foreach (var item in _freeSlices)
            {
                yield return new StorageSpaceUsageEntry(item);
            }

            foreach (var item in _notFullSlices)
            {
                yield return item.Value;
            }
        }
    }
}
