using System;
using Microsoft.Extensions.Caching.Memory;

namespace WB.UI.Designer.Code
{
    public static class QuestionnaireETagCache
    {
        // Keep latest-revision ETags hot for read-heavy editor traffic.
        public static readonly TimeSpan LatestRevisionTtl = TimeSpan.FromMinutes(2);

        public static string GetLatestRevisionCacheKey(Guid questionnaireId)
            => $"etag_seq_{questionnaireId:N}";

        public static void InvalidateLatestRevision(IMemoryCache memoryCache, Guid questionnaireId)
            => memoryCache.Remove(GetLatestRevisionCacheKey(questionnaireId));
    }
}