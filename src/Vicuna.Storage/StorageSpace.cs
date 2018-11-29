using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Vicuna.Storage
{
    public class StorageSpace
    {
        private StorageSegment _activeSegment;

        private readonly Stack<long> _freeSegments;

        private readonly HashSet<long> _fullSegments;

        private readonly StorageSliceFreeHandling _freeHandling;

        private readonly ConcurrentDictionary<long, StorageSegmentSpaceEntry> _notFullSegments;

        public StorageSegment ActiveSegment
        {
            get
            {
                if (_activeSegment == null)
                {
                    _activeSegment = AllocateSegment();
                }

                return _activeSegment;
            }
        }

        public StorageSpace()
        {
            _freeSegments = new Stack<long>();
            _fullSegments = new HashSet<long>();
            _freeHandling = new StorageSliceFreeHandling();
            _notFullSegments = new ConcurrentDictionary<long, StorageSegmentSpaceEntry>();
        }

        public bool Allocate(int size, out long loc)
        {
            if (ActiveSegment.Allocate(size, out loc))
            {
                return true;
            }

            for (var i = 0; i < _notFullSegments.Count; i++)
            {
                var entry = _notFullSegments[i];
                if (entry.UsedSize + size > StorageSegmentSpaceEntry.Capacity)
                {
                    continue;
                }

                var segment = GetSegment(entry.Loc);
                if (segment == null)
                {
                    continue;
                }

                if (segment.Allocate(size, out loc))
                {
                    return true;
                }
            }

            if (_notFullSegments.Count == 0)
            {
                _activeSegment = AllocateSegment();
                _notFullSegments.TryAdd(_activeSegment.Loc, new StorageSegmentSpaceEntry(_activeSegment.Loc)));
            }

            return _activeSegment.Allocate(size, out loc);
        }

        private StorageSegment GetSegment(StorageSegmentSpaceEntry entry)
        {
            return null;
        }

        private void FreeSegment(long loc)
        {
            if (_fullSegments.Contains(loc))
            {
                _fullSegments.Remove(loc);
            }

            if (_notFullSegments.ContainsKey(loc))
            {
                _notFullSegments.Remove(loc, out var _);
            }

            if (_freeSegments.Count < 4)
            {
                _freeSegments.Push(loc);
                return;
            }

            _freeHandling.Free(loc);
        }

        private StorageSegment AllocateSegment()
        {
            if (_freeSegments.Count > 0)
            {
                var entry = new StorageSegmentSpaceEntry(_freeSegments.Pop());
            }

            if (_freeHandling.Allocate(out var loc))
            {
                var entry = new StorageSegmentSpaceEntry(loc);
            }

            return null;
        }
    }
}
