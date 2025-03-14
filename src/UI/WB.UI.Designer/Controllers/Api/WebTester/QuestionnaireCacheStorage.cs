﻿#nullable enable
using WB.Core.GenericSubdomains.Portable;
using System;
using Microsoft.Extensions.Caching.Memory;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;

namespace WB.UI.Designer.Api.WebTester
{
    public interface IQuestionnaireCacheStorage
    {
        void Remove(string cacheKey);
        
        Questionnaire? GetOrCreate(string cacheKey, Guid questionnaireId, Func<Guid, Questionnaire> factory);
    }

    public class QuestionnaireCacheStorage : IQuestionnaireCacheStorage
    {
        private readonly IMemoryCache memoryCache;
        private readonly NamedLocker locker = new NamedLocker();

        public QuestionnaireCacheStorage(IMemoryCache memoryCache)
        {
            this.memoryCache = memoryCache;
        }

        private const string CachePrefix = "qcs::";

        public Questionnaire? GetOrCreate(string cacheKey, Guid questionnaireId, Func<Guid, Questionnaire> factory)
        {
            return this.locker.RunWithLock(cacheKey, () => memoryCache.GetOrCreate(GetKey(cacheKey), cache =>
            {
                cache.SetSlidingExpiration(TimeSpan.FromMinutes(5));
                return factory.Invoke(questionnaireId);
            }));
        }

        public void Remove(string cacheKey)
        { 
            this.locker.RunWithLock(cacheKey, () => memoryCache.Remove(GetKey(cacheKey)));
        }

        private string GetKey(string cacheKey)
        {
            return CachePrefix + cacheKey;
        }
    }
}
