﻿using System;
using Microsoft.Extensions.Caching.Memory;
using WB.Services.Export.Infrastructure;

namespace WB.Services.Export.Questionnaire.Services.Implementation
{
    internal class QuestionnaireStorageCache : IQuestionnaireStorageCache
    {
        private readonly IMemoryCache memoryCache;
        private readonly ITenantContext tenantContext;
        private string? keyPart;

        public QuestionnaireStorageCache(
            IMemoryCache memoryCache,
            ITenantContext tenantContext)
        {
            this.memoryCache = memoryCache;
            this.tenantContext = tenantContext;
        }

        private string Key(QuestionnaireId id, Guid? translation)
        {
            keyPart ??= nameof(QuestionnaireStorageCache) + ":" + tenantContext.Tenant.Id + ":";
            return keyPart + id + (translation != null ? $":{translation}" : string.Empty);
        }

        public bool TryGetValue(QuestionnaireId id, Guid? translation, out QuestionnaireDocument? document)
        {
            if (memoryCache.TryGetValue(Key(id, translation), out var res))
            {
                document = res as QuestionnaireDocument;
                if (document == null)
                    return false;

                return true;
            }

            document = null;
            return false;
        }

        public void Remove(QuestionnaireId id, Guid? translation)
        {
            memoryCache.Remove(Key(id, translation));
        }

        public void Set(QuestionnaireId id, Guid? translation, QuestionnaireDocument questionnaire)
        {
            memoryCache.Set(Key(id, translation), questionnaire, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(5)
            });
        }
    }
}
