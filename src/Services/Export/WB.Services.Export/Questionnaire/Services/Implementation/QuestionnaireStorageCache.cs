using System;
using Microsoft.Extensions.Caching.Memory;
using WB.Services.Export.Infrastructure;

namespace WB.Services.Export.Questionnaire.Services.Implementation
{
    internal class QuestionnaireStorageCache : IQuestionnaireStorageCache
    {
        private readonly TenantDbContext tenantDbContext;
        private readonly IMemoryCache memoryCache;
        private readonly ITenantContext tenantContext;
        private string keyPart;

        public QuestionnaireStorageCache(TenantDbContext tenantDbContext,
            IMemoryCache memoryCache,
            ITenantContext tenantContext)
        {
            this.tenantDbContext = tenantDbContext;
            this.memoryCache = memoryCache;
            this.tenantContext = tenantContext;
        }

        private string Key(QuestionnaireId id)
        {
            if (keyPart == null) keyPart = nameof(QuestionnaireStorageCache) + ":" + tenantContext.Tenant.Id + ":";
            return keyPart + id;
        }

        public bool TryGetValue(QuestionnaireId id, out QuestionnaireDocument document)
        {
            if (memoryCache.TryGetValue(Key(id), out var res))
            {
                document = res as QuestionnaireDocument;
                if (document == null) return false;

                // should be (and actually is) fast due to EF entity cache
                var db = this.tenantDbContext.GeneratedQuestionnaires.Find(id.ToString());

                if (db == null) return false;

                if (document.IsDeleted && db.DeletedAt == null) return false;

                if (!document.IsDeleted && db.DeletedAt != null) return false;

                return true;
            }

            document = null;
            return false;
        }

        public void Remove(QuestionnaireId id)
        {
            memoryCache.Remove(Key(id));
        }

        public void Set(QuestionnaireId id, QuestionnaireDocument questionnaire)
        {
            memoryCache.Set(Key(id), questionnaire, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(5)
            });
        }
    }
}
