using System;
using System.Collections.Generic;

namespace IconMan.Util
{
    public static class MoreLinq
    {
        public static int IndexWhere<T>(this IList<T> self, Predicate<T> predicate, int start = 0)
        {
            for (int i = start; i < self.Count; i++)
            {
                if (predicate(self[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        public static int RemoveFirstRangeWhere<T>(this IList<T> self, Predicate<T> predicate)
        {
            int start = self.IndexWhere(predicate);
            if (start < 0) { return 0; }

            int end = self.IndexWhere(e => !predicate(e), start);
            if (end < 0) { end = self.Count; }

            for (int i = end - 1; i >= start; --i)
            {
                self.RemoveAt(i);
            }

            return end - start;
        }
    }
}
