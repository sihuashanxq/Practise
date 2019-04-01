using System.Collections.Generic;

namespace Vicuna.Storage.Extensions
{
    public static class CollectionExtesions
    {
        public static HashSet<T> AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> items)
        {
            if (hashSet == null)
            {
                return hashSet;
            }

            foreach (var item in items)
            {
                hashSet.Add(item);
            }

            return hashSet;
        }
    }
}
