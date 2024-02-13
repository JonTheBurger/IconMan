using System;
using System.Collections.Generic;

namespace IconMan.Util
{
    /// <summary>
    /// Don't look at me like that. Everyone has one of these. How dare you read this docstring.
    /// </summary>
    public static class MoreLinq
    {
        /// <summary>
        /// Finds the first index in a list where the given predicate is true.
        /// </summary>
        /// <typeparam name="T">Element type of list.</typeparam>
        /// <param name="self">List to query.</param>
        /// <param name="predicate">Function that returns true upon the target search element.</param>
        /// <param name="start">Index from where the search will begin.</param>
        /// <returns>
        /// Index of the first element that matches <paramref name="predicate"/>, otherwise -1.
        /// </returns>
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

        /// <summary>
        /// Removes the first sequence of contiguous elements in a list that match a predicate.
        /// </summary>
        /// <typeparam name="T">Element type of list.</typeparam>
        /// <param name="self">List to query.</param>
        /// <param name="predicate">Function that returns true upon the target search element.</param>
        /// <returns>The number of elements that were removed.</returns>
        public static int RemoveFirstRangeWhere<T>(this IList<T> self, Predicate<T> predicate)
        {
            int start = self.IndexWhere(predicate);
            if (start < 0) { return 0; }

            int end = self.IndexWhere(e => !predicate(e), start);
            if (end < 0) { end = self.Count; }

            // We remove backwards to avoid paying for shifting doomed elemnts left.
            for (int i = end - 1; i >= start; --i)
            {
                self.RemoveAt(i);
            }

            return end - start;
        }
    }
}
