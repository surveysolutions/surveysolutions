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
        private readonly IWebTesterTranslationService translationService;

        public WebTesterQuestionnaireStorage(IWebTesterTranslationService translationService, 
            IQuestionOptionsRepository questionOptionsRepository,
            ISubstitutionService substitutionService)
        {
            this.translationService = translationService;
            this.questionOptionsRepository = questionOptionsRepository;
            this.substitutionService = substitutionService;
        }

        private readonly ConcurrentDictionary<string, PlainQuestionnaire> plainQuestionnairesCache 
            = new ConcurrentDictionary<string, PlainQuestionnaire>();

        private readonly IQuestionOptionsRepository questionOptionsRepository;
        private readonly ISubstitutionService substitutionService;

        public IQuestionnaire GetQuestionnaire(QuestionnaireIdentity identity, string language)
        {
            if (this.plainQuestionnairesCache.TryGetValue(identity.ToString(), out PlainQuestionnaire q))
            {
                if (language == null)
                {
                    return q;
                }
            }

            return this.translationService.Translate(q, identity.Version, language);
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
