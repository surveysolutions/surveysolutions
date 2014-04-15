using System;
using System.Net.Http.Headers;

namespace WB.UI.Headquarters.API.Feeds
{
    internal static class Constants
    {
        public static CacheControlHeaderValue DefaultArchiveCache()
        {
            return new CacheControlHeaderValue
            {
                MaxAge = TimeSpan.FromDays(100),
                Public = true
            };
        }
    }
}