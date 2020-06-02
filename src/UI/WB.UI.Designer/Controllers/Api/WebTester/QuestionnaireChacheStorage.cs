using System;
using Microsoft.Extensions.Caching.Memory;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;

namespace WB.UI.Designer.Api.WebTester
{
    public interface IQuestionnaireCacheStorage
    {
        Lazy<Questionnaire>? Get(string cacheKey);
        void Add(string cacheKey, Lazy<Questionnaire> cacheEntry);
        void Remove(string cacheKey);
    }

    public class QuestionnaireCacheStorage : IQuestionnaireCacheStorage
    {
        private readonly IMemoryCache memoryCache;

        public QuestionnaireCacheStorage(IMemoryCache memoryCache)
        {
            this.memoryCache = memoryCache;
        }

        private const string CachePrefix = "qcs::";

        public Lazy<Questionnaire>? Get(string cacheKey)
        {
            return memoryCache.Get(CachePrefix + cacheKey) as Lazy<Questionnaire>;
        }

        public void Add(string cacheKey, Lazy<Questionnaire> cacheEntry)
        {
            memoryCache.Set(CachePrefix + cacheKey, cacheEntry, new MemoryCacheEntryOptions()
            {
                SlidingExpiration = TimeSpan.FromMinutes(10)
            });
        }

        public void Remove(string cacheKey)
        {
            memoryCache.Remove(CachePrefix + cacheKey);
        }
    }
}
