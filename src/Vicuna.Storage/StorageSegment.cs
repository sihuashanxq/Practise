using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vicuna.Storage.Pages;

namespace Vicuna.Storage
{
    public class StorageSegment
    {
        private StoragePage _storagePage;

        private StorageSlice _lastUsedSlice;

        private StorageSpaceEntry _usage;

        private readonly Queue<long> _freeSlices;

        private readonly HashSet<long> _fullSlices;

        private readonly StorageSliceManager _sliceHandling;

        private readonly ConcurrentDictionary<long, StorageSpaceEntry> _notFullSlices;

        public long Loc { get; set; }

        internal StoragePage StoragePage => _storagePage;

        internal StorageSlice LastUsedSlice => _lastUsedSlice ?? (_lastUsedSlice = GetSlice());

        internal StorageSpaceEntry Usage => _usage;

        public StorageSegment(StoragePage storagePage, StorageSliceManager sliceHandling)
        {
            _storagePage = storagePage;
            _sliceHandling = sliceHandling;
            _freeSlices = new Queue<long>();
            _fullSlices = new HashSet<long>();
            _usage = new StorageSpaceEntry();
            _notFullSlices = new ConcurrentDictionary<long, StorageSpaceEntry>();

            for (var i = 0; i < _storagePage.ItemCount; i++)
            {
                _notFullSlices[_storagePage.GetEntry(64).Pos] = _storagePage.GetEntry(64);
                _lastUsedSlice = _sliceHandling.GetSlice(_storagePage.GetEntry(64).Pos);
            }
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

                var slice = _sliceHandling.GetSlice(entry.Pos);
                if (slice == null)
                {
                    continue;
                }

                if (Allocate(slice, size, out buffer))
                {
                    return true;
                }
            }

            var allocatedSlice = GetSlice();
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
                _notFullSlices.TryRemove(storageSlice.StroageSlicePage.PagePos, out var _);
            }
            else
            {
                _lastUsedSlice = storageSlice;
                _notFullSlices[storageSlice.StroageSlicePage.PagePos] = storageSlice.Usage;
            }

            return true;
        }

        public bool AllocatePage(int pageCount, out Page[] pages)
        {
            if (pageCount > 1024)
            {
                throw new InvalidOperationException($"allocated page count:{pageCount} more than 1024 at once!");
            }

            if (AllocatePage(LastUsedSlice, pageCount, out pages))
            {
                return true;
            }

            foreach (var entry in _notFullSlices.Values.ToList())
            {
                if (entry.UsedSize + Constants.PageSize * pageCount > Constants.StorageSliceSize)
                {
                    continue;
                }

                var slice = _sliceHandling.GetSlice(entry.Pos);
                if (slice == null)
                {
                    continue;
                }

                if (AllocatePage(slice, pageCount, out pages))
                {
                    return true;
                }
            }

            var allocatedSlice = GetSlice();
            if (allocatedSlice == null)
            {
                return false;
            }

            return AllocatePage(allocatedSlice, pageCount, out pages);
        }

        private bool AllocatePage(StorageSlice storageSlice, int pageCount, out Page[] pages)
        {
            if (storageSlice == null)
            {
                pages = null;
                return false;
            }

            if (!storageSlice.AllocatePage(pageCount, out pages))
            {
                return false;
            }

            if (storageSlice.Usage.UsedSize == Constants.StorageSliceSize)
            {
                _lastUsedSlice = null;
                _fullSlices.Add(storageSlice.StroageSlicePage.PagePos);
                _notFullSlices.TryRemove(storageSlice.StroageSlicePage.PagePos, out var _);
            }
            else
            {
                _lastUsedSlice = storageSlice;
                _notFullSlices[storageSlice.StroageSlicePage.PagePos] = storageSlice.Usage;
            }

            return true;
        }

        private StorageSlice GetSlice()
        {
            if (_freeSlices.Count > 0)
            {
                return _sliceHandling.GetSlice(_freeSlices.Dequeue());
            }

            if (_notFullSlices.Count + _fullSlices.Count < 512)
            {
                var slice = _sliceHandling.Allocate();

                _notFullSlices[slice.StroageSlicePage.PagePos] = slice.Usage;

                return slice;
            }

            return null;
        }

        internal IEnumerable<StorageSpaceEntry> GetSlices()
        {
            foreach (var item in _fullSlices)
            {
                yield return new StorageSpaceEntry(item, 0);
            }

            foreach (var item in _freeSlices)
            {
                yield return new StorageSpaceEntry(item);
            }

            foreach (var item in _notFullSlices)
            {
                yield return item.Value;
            }
        }
    }
}
