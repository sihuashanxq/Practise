using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Vicuna.Storage
{
    public class StoragePageCachePool
    {
        private readonly StoragePageCachePool _root;

        private readonly HashSet<long> _freedPages;

        private readonly ConcurrentDictionary<long, byte[]> _cachedPages;

        private readonly ConcurrentDictionary<long, byte[]> _modifiedPages;

        private readonly ConcurrentDictionary<long, byte[]> _allocatedPages;

        public StoragePageCachePool(StoragePageCachePool root)
        {
            _root = root;
            _freedPages = new HashSet<long>();
            _cachedPages = new ConcurrentDictionary<long, byte[]>();
            _modifiedPages = new ConcurrentDictionary<long, byte[]>();
            _allocatedPages = new ConcurrentDictionary<long, byte[]>();
        }

        public StoragePageCachePool CreatePool()
        {
            return new StoragePageCachePool(_root);
        }

        public bool TryGetPage(long pageOffset, out byte[] buffer)
        {
            if (_allocatedPages.TryGetValue(pageOffset, out buffer))
            {
                return true;
            }

            if (_modifiedPages.TryGetValue(pageOffset, out buffer))
            {
                return true;
            }

            if (_cachedPages.TryGetValue(pageOffset, out buffer))
            {
                return true;
            }

            if (_root != null)
            {
                return _root.TryGetPage(pageOffset, out buffer);
            }

            return false;
        }

        public bool TryGetPageToModify(long pageOffset, out byte[] buffer)
        {
            if (_allocatedPages.TryGetValue(pageOffset, out buffer))
            {
                return true;
            }

            if (_modifiedPages.TryGetValue(pageOffset, out buffer))
            {
                return true;
            }

            if (_cachedPages.TryGetValue(pageOffset, out buffer))
            {
                var mBuffer = new byte[Constants.PageSize];

                Array.Copy(buffer, mBuffer, Constants.PageSize);

                _modifiedPages.TryAdd(pageOffset, mBuffer);

                return true;
            }

            if (_root != null && _root.TryGetPage(pageOffset, out buffer))
            {
                var mBuffer = new byte[Constants.PageSize];

                Array.Copy(buffer, mBuffer, Constants.PageSize);

                _modifiedPages.TryAdd(pageOffset, mBuffer);
            }

            return false;
        }
    }
}
