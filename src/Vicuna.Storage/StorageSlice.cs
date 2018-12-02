using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Vicuna.Storage.Pages;

namespace Vicuna.Storage
{
    public class StorageSlice
    {
        public long Loc { get; }

        private Page _lastUsedPage;

        private readonly Stack<long> _freePages;

        private readonly HashSet<long> _fullPages;

        private readonly ConcurrentDictionary<long, StoragePageSpaceEntry> _notFullPages;

        internal Page LastUsedPage
        {
            get
            {
                if (_lastUsedPage == null)
                {
                    _lastUsedPage = new Page();
                }

                return _lastUsedPage;
            }
        }

        internal StorageSliceSpaceEntry SpaceEntry { get; set; }

        public StorageSlice(long loc)
        {
            _freePages = new Stack<long>();
            _fullPages = new HashSet<long>();
            _notFullPages = new ConcurrentDictionary<long, StoragePageSpaceEntry>();
        }

        public bool Allocate(int size, out AllocatedPageBuffer buffer)
        {
            buffer = null;
            return false;
        }
    }

    internal class StoragePageSpaceEntry
    {
        public short Offset { get; internal set; }

        public short UsedSize { get; internal set; }

        public StoragePageSpaceEntry(short offset)
            : this(offset, 0)
        {
        }

        public StoragePageSpaceEntry(short offset, short usedSize)
        {
            Offset = offset;
            UsedSize = usedSize;
        }
    }

    internal class StorageSliceSpaceEntry
    {
        public const int Capacity = 1024 * 1024;

        public long Loc { get; internal set; }

        public int UsedSize { get; internal set; }

        public StorageSliceSpaceEntry(long loc)
            : this(loc, 0)
        {

        }

        public StorageSliceSpaceEntry(long loc, int usedSize)
        {
            Loc = loc;
            UsedSize = usedSize;
        }
    }
}

public class AllocatedPageBuffer
{
    public short Offset { get; set; }

    public PageHeader Header { get; set; }

    public Memory<byte> Buffer { get; set; }

    public AllocatedPageBuffer(PageHeader heaer, short offset, Memory<byte> buffer)
    {

    }
}