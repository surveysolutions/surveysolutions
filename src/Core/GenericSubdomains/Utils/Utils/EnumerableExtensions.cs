using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.GenericSubdomains.Utils
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> ToEnumerable<T>(this T element)
        {
            yield return element;
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
    }
}