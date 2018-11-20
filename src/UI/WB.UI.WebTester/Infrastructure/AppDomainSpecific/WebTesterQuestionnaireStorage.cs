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

        public PlainQuestionnaire Questionnaire { get; set; }

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
            string questionnaireCacheKey = language != null ? $"{identity}${language}" : $"{identity}";

            return this.plainQuestionnairesCache.GetOrAdd(questionnaireCacheKey,
                s => this.translationStorage.Translate(Questionnaire.QuestionnaireDocument, identity.Version, language));
        }
        
        public void StoreQuestionnaire(Guid id, long version, QuestionnaireDocument questionnaireDocument)
        {
            Questionnaire = new PlainQuestionnaire(questionnaireDocument, version, questionOptionsRepository, substitutionService);
        }

        public QuestionnaireDocument GetQuestionnaireDocument(QuestionnaireIdentity identity)
        {
            return Questionnaire.QuestionnaireDocument;
        }

        public QuestionnaireDocument GetQuestionnaireDocument(Guid id, long version)
        {
            return Questionnaire.QuestionnaireDocument;
        }

        public void DeleteQuestionnaireDocument(Guid id, long version)
        {
            Questionnaire = null;
        }
    }
}
