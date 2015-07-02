using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace WB.Core.GenericSubdomains.Portable
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> WithoutLast<T>(this IEnumerable<T> source)
        {
            using (var e = source.GetEnumerator())
            {
                if (e.MoveNext())
                {
                    for (var value = e.Current; e.MoveNext(); value = e.Current)
                    {
                        yield return value;
                    }
                }
            }
        }

        public static IEnumerable<T> ToEnumerable<T>(this T element)
        {
            yield return element;
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            return new HashSet<T>(source);
        }

        public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this IList<T> source)
        {
            return new ReadOnlyCollection<T>(source);
        }

        public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> source)
        {
            return source.ToList().ToReadOnlyCollection();
        }

        public static IEnumerable<T> Except<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
        {
            return enumerable.Where(element => !predicate.Invoke(element));
        }

        public static IEnumerable<TResult> SelectUsingPrevCurrNext<TSource, TResult>(this IEnumerable<TSource> enumerable, Func<TSource, TSource, TSource, TResult> selector)
            where TSource : class
        {
            var enumerator = enumerable.GetEnumerator();
            if (!enumerator.MoveNext())
                yield break;

            TSource current = enumerator.Current;

            if (!enumerator.MoveNext())
            {
                yield return selector.Invoke(null, current, null);
                yield break;
            }

            TSource next = enumerator.Current;

            yield return selector.Invoke(null, current, next);

            while (enumerator.MoveNext())
            {
                TSource prev = current;
                current = next;
                next = enumerator.Current;

                yield return selector.Invoke(prev, current, next);
            }

            yield return selector.Invoke(current, next, null);
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
    }
}