using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Enumerator.Native.Questionnaire;

namespace WB.UI.WebTester.Infrastructure
{
    class WebTesterTranslationService : IWebTesterTranslationService
    {
        private readonly IQuestionOptionsRepository questionOptionsRepository;
        private readonly ISubstitutionService substitutionService;
        private readonly IWebTesterTranslationStorage storage;
        private static readonly MemoryCache translationsCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

        public WebTesterTranslationService(IWebTesterTranslationStorage storage, 
            IQuestionOptionsRepository questionOptionsRepository,
            ISubstitutionService substitutionService)
        {
            this.storage = storage;
            this.questionOptionsRepository = questionOptionsRepository;
            this.substitutionService = substitutionService;
        }

        public PlainQuestionnaire? Translate(PlainQuestionnaire questionnaire, long version, string? language)
        {
            return translationsCache.GetOrCreate($"{questionnaire.QuestionnaireIdentity}${language}", (entry) =>
            {
                QuestionnaireDocument result = storage.GetTranslated(questionnaire.QuestionnaireDocument, version,
                    language, out Translation? translation);
                var plainQuestionnaire = new PlainQuestionnaire(result, version, questionOptionsRepository, substitutionService, translation);
                plainQuestionnaire.ExpressionStorageType = questionnaire.ExpressionStorageType;
                plainQuestionnaire.WarmUpPriorityCaches();

                entry.SlidingExpiration = TimeSpan.FromSeconds(15);

                return plainQuestionnaire;
            });
        }
    }
}
