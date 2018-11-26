using System.Collections.Generic;

namespace Vicuna.Storage
{
    public class StorageSpace
    {
        private List<StorageSegmentEntry> _freeSegments;

        private List<StorageSegmentEntry> _usedSegments;

        public StorageSpace()
        {
            _usedSegments = new List<StorageSegmentEntry>();
            _freeSegments = new List<StorageSegmentEntry>();
        }

        private void FreeSegment(long loc)
        {
            _freeSegments.Add(new StorageSegmentEntry(loc));
        }

        private StorageSegmentEntry AllocNewSegment()
        {
            return null;
        }

        public StorageSegment GetHasFreeSpaceSegment(int size)
        {
            return null;
        }
    }
}
