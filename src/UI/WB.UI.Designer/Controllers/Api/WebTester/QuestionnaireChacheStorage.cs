#nullable enable
using System;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;

namespace WB.UI.Designer.Api.WebTester
{
    public interface IQuestionnaireCacheStorage
    {
        void Remove(Guid questionnaireId);
        
        Questionnaire? GetOrCreate(Guid questionnaireId, Func<Guid, Guid, Questionnaire> factory);
    }

    public class QuestionnaireCacheStorage : IQuestionnaireCacheStorage
    {
        private readonly IMemoryCache memoryCache;
        private readonly DesignerDbContext dbContext;

        public QuestionnaireCacheStorage(IMemoryCache memoryCache, DesignerDbContext dbContext)
        {
            this.memoryCache = memoryCache;
            this.dbContext = dbContext;
        }

        private const string CachePrefix = "qcs::";


        public Questionnaire? GetOrCreate(Guid questionnaireId, Func<Guid, Guid, Questionnaire> factory)
        {
            var cacheKey = GetCacheKey(questionnaireId, out var originalQuestionnaireId);

            return memoryCache.GetOrCreate(GetKey(cacheKey), cache =>
            {
                cache.SetSlidingExpiration(TimeSpan.FromMinutes(5));
                return factory.Invoke(originalQuestionnaireId, questionnaireId);
            });
        }

        private string GetCacheKey(Guid questionnaireId, out Guid originalQuestionnaireId)
        {
            var anonymousQuestionnaire = this.dbContext.AnonymousQuestionnaires.FirstOrDefault(a => a.IsActive
                && a.AnonymousQuestionnaireId == questionnaireId);
            originalQuestionnaireId = anonymousQuestionnaire?.QuestionnaireId ?? questionnaireId;
            var strOrigId = originalQuestionnaireId.FormatGuid();

            var maxSequenceByQuestionnaire = this.dbContext.QuestionnaireChangeRecords
                .Where(y => y.QuestionnaireId == strOrigId)
                .Select(y => (int?)y.Sequence)
                .Max();
            
            string cacheKey = $"{questionnaireId}.{maxSequenceByQuestionnaire}";
            return cacheKey;
        }

        public void Remove(Guid questionnaireId)
        {
            var cacheKey = GetCacheKey(questionnaireId, out _);
            memoryCache.Remove(GetKey(cacheKey));
        }
        
        private string GetKey(string cacheKey)
        {
            return CachePrefix + cacheKey;
        }
    }
}
