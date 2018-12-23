using System.Collections.Generic;
using System.Linq;

namespace Vicuna.Storage
{
    public class StorageSliceFreeHandling
    {
        private readonly HashSet<long> _freePages;

        public StorageSliceFreeHandling()
        {
            _freePages = new HashSet<long>();
        }

        public bool Allocate(out long loc)
        {
            if (_freePages.Count == 0)
            {
                loc = -1;
                return false;
            }

            loc = _freePages.First();

            return _freePages.Remove(loc);
        }

        public void Free(long loc)
        {
            _freePages.Add(loc);
        }
    }
}
