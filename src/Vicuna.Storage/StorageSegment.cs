using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Vicuna.Storage
{
    public class StorageSegment
    {
        private StoragePage _storagePage;

        private StorageSlice _lastUsedSlice;

        private StorageSpaceUsageEntry _usage;

        private readonly Queue<long> _freeSlices;

        private readonly HashSet<long> _fullSlices;

        private readonly StorageSliceHandling _sliceHandling;

        private readonly ConcurrentDictionary<long, StorageSpaceUsageEntry> _notFullSlices;

        public long Loc { get; set; }

        internal StoragePage StoragePage => _storagePage;

        internal StorageSlice LastUsedSlice => _lastUsedSlice;

        internal StorageSpaceUsageEntry Usage => _usage;

        public StorageSegment(
            StoragePage storagePage,
            StorageSliceHandling sliceHandling
        )
        {
            _storagePage = storagePage;
            _sliceHandling = sliceHandling;
            _freeSlices = new Queue<long>();
            _fullSlices = new HashSet<long>();
            _usage = new StorageSpaceUsageEntry();
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

            var allocatedSlice = AllocateSlice();
            if (allocatedSlice == null)
            {
                return false;
            }

            return Allocate(allocatedSlice, size, out buffer);
        }

        private bool Allocate(StorageSlice storageSlice, int size, out AllocationBuffer buffer)
        {
            if (storageSlice == null)
            {
                buffer = null;
                return false;
            }

            if (!storageSlice.Allocate(size, out buffer))
            {
                return false;
            }

            if (storageSlice.Usage.UsedSize == Constants.StorageSliceSize)
            {
                _lastUsedSlice = null;
                _fullSlices.Add(storageSlice.StroageSlicePage.PagePos);
                _notFullSlices.TryRemove(storageSlice.Loc, out var _);
            }
            else
            {
                _lastUsedSlice = storageSlice;
                _notFullSlices[storageSlice.StroageSlicePage.PagePos] = storageSlice.Usage;
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
            if (_freeSlices.Count > 0)
            {
                return _sliceHandling.GetSlice(_freeSlices.Dequeue());
            }

            if (_notFullSlices.Count + _fullSlices.Count < 512)
            {
                return _sliceHandling.AllocateSlice();
            }

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
