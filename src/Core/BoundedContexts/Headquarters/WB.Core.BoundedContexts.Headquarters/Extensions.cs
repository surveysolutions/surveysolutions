using System;
using System.Globalization;
using Microsoft.Extensions.Caching.Memory;

namespace WB.Core.BoundedContexts.Headquarters
{
    public static class Extensions
    {
        public static string ClientDateTimeFormat = "MMM DD, YYYY HH:mm";
        public static string ServerDateTimeFormat = "MMM dd, yyy HH:mm";

        public static string FormatDateWithTime(this DateTime dateTime) 
            => dateTime.ToString(ServerDateTimeFormat, CultureInfo.CurrentUICulture);

        /// <summary>
        /// Override over GetOrCreate that will handle null result from cache entry factory, and not store it
        /// </summary>
        /// <returns>Cached item or non cached null</returns>
        public static TItem GetOrCreateNullSafe<TItem>(this IMemoryCache cache, object key, Func<ICacheEntry, TItem> factory)
            where TItem: class
        {
            if (!cache.TryGetValue(key, out object result))
            {
                var entry = cache.CreateEntry(key);
                result = factory(entry);

                if (result == null)
                {
                    return null;
                }

                entry.SetValue(result);
                // need to manually call dispose instead of having a using
                // in case the factory passed in throws, in which case we
                // do not want to add the entry to the cache
                entry.Dispose();
            }

            return (TItem)result;
        }
    }
}
