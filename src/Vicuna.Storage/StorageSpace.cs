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

        private readonly ConcurrentDictionary<long, StorageSpaceUsageEntry> _notFullSegments;

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
            _notFullSegments = new ConcurrentDictionary<long, StorageSpaceUsageEntry>();
        }

        public bool Allocate(int size, out AllocationBuffer buffer)
        {
            if (Allocate(ActiveSegment, size, out buffer))
            {
                return true;
            }

            foreach (var spaceEntry in _notFullSegments.Values.ToList())
            {
                if (spaceEntry.UsedSize + size > 1024L * 16 * 1024 * 512)
                {
                    continue;
                }

                var segment = GetSegment(spaceEntry);
                if (segment == null)
                {
                    continue;
                }

                if (Allocate(segment, size, out buffer))
                {
                    return true;
                }
            }

            return Allocate(AllocateSegment(), size, out buffer);
        }

        /// <summary>
        /// 从给定Segment中分配 size长度的空间
        /// </summary>
        /// <param name="segment">Segment</param>
        /// <param name="size">需要分配的长度</param>
        /// <param name="buffer">分配空间的起始地址</param>
        /// <returns></returns>
        private bool Allocate(StorageSegment segment, int size, out AllocationBuffer buffer)
        {
            if (segment == null)
            {
                buffer = null;
                return false;
            }

            if (!segment.Allocate(size, out buffer))
            {
                return false;
            }

            if (segment.Usage.UsedSize == 1024L * 1024L * 16L * 512L/* *0.95 */)
            {
                _fullSegments.Add(segment.Loc);
                _notFullSegments.TryRemove(segment.Loc, out var _);
            }
            else
            {
                _notFullSegments[segment.Loc] = segment.Usage;
            }

            _activeSegment = segment;

            return true;
        }

        private StorageSegment GetSegment(StorageSpaceUsageEntry entry)
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
                _freeHandling.Free(slice.Pos);
            }
        }

        private StorageSegment AllocateSegment()
        {
            if (_freeHandling.Allocate(out var loc))
            {

            }

            return null;
        }
    }
}
