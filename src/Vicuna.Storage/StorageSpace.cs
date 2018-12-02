using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

namespace Vicuna.Storage
{
    public class StorageSpace
    {
        public const int MaxSegmentCount = 512;

        public const int MaxFreeSegmentCount = 4;

        private StorageSegment _activeSegment;

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
            _fullSegments = new HashSet<long>();
            _freeHandling = new StorageSliceFreeHandling();
            _notFullSegments = new ConcurrentDictionary<long, StorageSegmentSpaceEntry>();
        }

        public bool Allocate(int size, out long loc)
        {
            if (Allocate(ActiveSegment, size, out loc))
            {
                return true;
            }

            foreach (var spaceEntry in _notFullSegments.Values.ToList())
            {
                if (spaceEntry.UsedSize + size > StorageSegmentSpaceEntry.Capacity)
                {
                    continue;
                }

                var segment = GetSegment(spaceEntry);
                if (segment == null)
                {
                    continue;
                }

                if (Allocate(segment, size, out loc))
                {
                    return true;
                }
            }

            var newSegment = AllocateSegment();
            if (newSegment == null)
            {
                loc = -1;
                return false;
            }

            return Allocate(newSegment, size, out loc);
        }

        /// <summary>
        /// 从给定Segment中分配 size长度的空间
        /// </summary>
        /// <param name="segment">Segment</param>
        /// <param name="size">需要分配的长度</param>
        /// <param name="loc">分配空间的起始地址</param>
        /// <returns></returns>
        private bool Allocate(StorageSegment segment, int size, out long loc)
        {
            if (segment == null)
            {
                loc = -1;
                return false;
            }

            if (!segment.Allocate(size, out loc))
            {
                return false;
            }

            if (segment.SpaceEntry.UsedSize == StorageSegmentSpaceEntry.Capacity/* *0.95 */)
            {
                _fullSegments.Add(segment.Loc);
                _notFullSegments.TryRemove(segment.Loc, out var _);
            }
            else
            {
                _notFullSegments[segment.Loc] = segment.SpaceEntry;
            }

            _activeSegment = segment;

            return true;
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

            if (!_notFullSegments.TryRemove(loc, out var spaceEntry))
            {
                return;
            }

            var freedSegment = GetSegment(spaceEntry);
            if (freedSegment == null)
            {
                throw null;
            }

            foreach (var slice in freedSegment.GetSlices())
            {
                _freeHandling.Free(slice.Loc);
            }
        }

        private StorageSegment AllocateSegment()
        {
            if (_freeHandling.Allocate(out var loc))
            {
                var entry = new StorageSegmentSpaceEntry(loc);
            }

            return null;
        }
    }
}
