using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Capi
{
    internal static class LinqUtils
    {
        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach (var item in enumeration)
            {
                action(item);
            }
        }
    }
}