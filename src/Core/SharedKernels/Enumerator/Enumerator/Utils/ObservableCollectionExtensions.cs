using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace WB.Core.SharedKernels.Enumerator.Utils
{
    public static class ObservableCollectionExtensions 
    {
        /// <summary>
        /// Synchronizes source collection with the target to get identical lists.
        /// </summary>
        /// <param name="source">Collection to modify during synchronization.</param>
        /// <param name="target">Target collection to receive.</param>
        /// <param name="comparer">Equality comparer for the items.</param>
        /// <returns>Returns items that were removed from the <paramref name="source"/> collection to properly dispose them.</returns>
        public static IEnumerable<T> SynchronizeWith<T>(this ObservableCollection<T> source, 
            IList<T> target,
            Func<T, T, bool> comparer)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));

            for (int i = 0; i < target.Count; i++)
            {
                if (i < source.Count)
                {
                    if (!comparer(source[i], target[i]))
                    {
                        source.Insert(i, target[i]);
                    }
                }
                else
                {
                    source.Add(target[i]);
                }
            }

            if (source.Count > target.Count)
            {
                var itemsToRemove =
                    source.Select((item, index) => new { index, item })
                        .Where(x => x.index >= target.Count)
                        .ToList();

                itemsToRemove.ForEach(option =>
                {
                    source.Remove(option.item);
                });

                return itemsToRemove.Select(x => x.item);
            }

            return Enumerable.Empty<T>();
        }
    }
}