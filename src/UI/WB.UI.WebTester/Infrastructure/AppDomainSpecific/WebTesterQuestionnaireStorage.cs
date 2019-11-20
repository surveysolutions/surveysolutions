using System;
using System.Collections.Concurrent;
using Main.Core.Documents;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.UI.WebTester.Infrastructure
{
    public class WebTesterQuestionnaireStorage : IQuestionnaireStorage
    {
        private readonly IWebTesterTranslationService translationStorage;

        public WebTesterQuestionnaireStorage(IWebTesterTranslationService translationStorage, 
            IQuestionOptionsRepository questionOptionsRepository,
            ISubstitutionService substitutionService)
        {
            this.translationStorage = translationStorage;
            this.questionOptionsRepository = questionOptionsRepository;
            this.substitutionService = substitutionService;
        }

        private readonly ConcurrentDictionary<string, PlainQuestionnaire> plainQuestionnairesCache 
            = new ConcurrentDictionary<string, PlainQuestionnaire>();

        private readonly IQuestionOptionsRepository questionOptionsRepository;
        private readonly ISubstitutionService substitutionService;

        public IQuestionnaire GetQuestionnaire(QuestionnaireIdentity identity, string language)
        {
            if (language == null)
            {
                if (this.plainQuestionnairesCache.TryGetValue(identity.ToString(), out PlainQuestionnaire q))
                {
                    return q;
                }
            }

            string questionnaireCacheKey = $"{identity}${language}";

            if (this.plainQuestionnairesCache.TryGetValue(questionnaireCacheKey, out PlainQuestionnaire q1))
            {
                return q1;
            }

            if (this.plainQuestionnairesCache.TryGetValue(identity.ToString(), out PlainQuestionnaire q3))
            {
                var result = this.translationStorage.Translate(q3, identity.Version,
                    language);
                this.plainQuestionnairesCache[questionnaireCacheKey] = result;
                return result;
            }

            return null;
        }
        
        public void StoreQuestionnaire(Guid id, long version, QuestionnaireDocument questionnaireDocument)
        {
            this.plainQuestionnairesCache[new QuestionnaireIdentity(id, version).ToString()] = 
                 new PlainQuestionnaire(questionnaireDocument, version, questionOptionsRepository, substitutionService);
        }

        public QuestionnaireDocument GetQuestionnaireDocument(QuestionnaireIdentity identity)
        {
            if (this.plainQuestionnairesCache.TryGetValue(identity.ToString(), out PlainQuestionnaire q))
            {
                return q.QuestionnaireDocument;
            }

            return null;
        }

        public QuestionnaireDocument GetQuestionnaireDocument(Guid id, long version)
        {
            return this.GetQuestionnaireDocument(new QuestionnaireIdentity(id, version));
        }

        public void DeleteQuestionnaireDocument(Guid id, long version)
        {
            this.plainQuestionnairesCache.TryRemove(new QuestionnaireIdentity(id, version).ToString(), out _);
        }
    }
}
