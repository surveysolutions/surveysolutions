using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace WB.Core.GenericSubdomains.Portable
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> GetCommonPart<T>(this IEnumerable<T> vector1, IEnumerable<T> vector2)
            where T : struct
            => vector1.Zip(vector2, (x, y) => x.Equals(y) ? x as T? : null as T?).TakeWhile(x => x != null).Select(x => x.Value);

        public static IEnumerable<T> ToEnumerable<T>(this T element)
        {
            yield return element;
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            return new HashSet<T>(source);
        }

        public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this IList<T> source)
            => source as ReadOnlyCollection<T>
            ?? new ReadOnlyCollection<T>(source);

        public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> source)
            => source as ReadOnlyCollection<T>
            ?? source.ToList().ToReadOnlyCollection();

        public static IEnumerable<T> Except<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
        {
            return enumerable.Where(element => !predicate.Invoke(element));
        }

        public static string GetOrderRequestString(this IEnumerable<OrderRequestItem> orders)
        {
            return orders == null
                ? String.Empty
                : String.Join(",",
                    orders.Select(o => o.Field + (o.Direction == OrderDirection.Asc ? String.Empty : " Desc")));
        }

        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach (T item in enumeration)
            {
                action(item);
            }
        }

        public static IEnumerable<T> ConcatSingle<T>(this IEnumerable<T> source, T item)
        {
            foreach (var sourceItem in source)
            {
                yield return sourceItem;
            }

            yield return item;
        }

        /// <summary>
        /// Batches the source sequence into sized buckets.
        /// </summary>
        /// <typeparam name="TSource">Type of elements in <paramref name="source"/> sequence.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="size">Size of buckets.</param>
        /// <returns>A sequence of equally sized buckets containing elements of the source collection.</returns>
        /// <remarks>
        /// This operator uses deferred execution and streams its results (buckets and bucket content). 
        /// </remarks>
        public static IEnumerable<IEnumerable<TSource>> Batch<TSource>(this IEnumerable<TSource> source, int size)
        {
            return Batch(source, size, x => x);
        }

        /// <summary>
        /// Batches the source sequence into sized buckets and applies a projection to each bucket.
        /// </summary>
        /// <typeparam name="TSource">Type of elements in <paramref name="source"/> sequence.</typeparam>
        /// <typeparam name="TResult">Type of result returned by <paramref name="resultSelector"/>.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="size">Size of buckets.</param>
        /// <param name="resultSelector">The projection to apply to each bucket.</param>
        /// <returns>A sequence of projections on equally sized buckets containing elements of the source collection.</returns>
        /// <remarks>
        /// This operator uses deferred execution and streams its results (buckets and bucket content).
        /// </remarks>
        public static IEnumerable<TResult> Batch<TSource, TResult>(this IEnumerable<TSource> source, int size,
            Func<IEnumerable<TSource>, TResult> resultSelector)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size));
            if (resultSelector == null) throw new ArgumentNullException(nameof(resultSelector));
            return BatchImpl(source, size, resultSelector);
        }

        private static IEnumerable<TResult> BatchImpl<TSource, TResult>(this IEnumerable<TSource> source, int size,
            Func<IEnumerable<TSource>, TResult> resultSelector)
        {
            TSource[] bucket = null;
            var count = 0;

            foreach (var item in source)
            {
                if (bucket == null)
                {
                    bucket = new TSource[size];
                }

                bucket[count++] = item;

                // The bucket is fully buffered before it's yielded
                if (count != size)
                {
                    continue;
                }

                // Select is necessary so bucket contents are streamed too
                yield return resultSelector(bucket.Select(x => x));

                bucket = null;
                count = 0;
            }

            // Return the last bucket with all remaining elements
            if (bucket != null && count > 0)
            {
                yield return resultSelector(bucket.Take(count));
            }
        }
    }
}
