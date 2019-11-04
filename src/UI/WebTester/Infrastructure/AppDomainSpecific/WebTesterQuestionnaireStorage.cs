using System;
using System.Collections.Concurrent;
using Main.Core.Documents;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;

namespace WB.UI.WebTester.Infrastructure
{
    public class WebTesterQuestionnaireStorage : IQuestionnaireStorage
    {
        private readonly IWebTesterTranslationService translationStorage;

        public PlainQuestionnaire Questionnaire { get; set; }

        public WebTesterQuestionnaireStorage(IWebTesterTranslationService translationStorage, 
            IQuestionOptionsRepository questionOptionsRepository,
            ISubstitutionService substitutionService,
            IInterviewExpressionStatePrototypeProvider prototypeProvider)
        {
            this.translationStorage = translationStorage;
            this.questionOptionsRepository = questionOptionsRepository;
            this.substitutionService = substitutionService;
            this.prototypeProvider = prototypeProvider;
        }

        private readonly ConcurrentDictionary<string, PlainQuestionnaire> plainQuestionnairesCache 
            = new ConcurrentDictionary<string, PlainQuestionnaire>();

        private readonly IQuestionOptionsRepository questionOptionsRepository;
        private readonly ISubstitutionService substitutionService;
        private readonly IInterviewExpressionStatePrototypeProvider prototypeProvider;

        public IQuestionnaire GetQuestionnaire(QuestionnaireIdentity identity, string language)
        {
            string questionnaireCacheKey = language != null ? $"{identity}${language}" : $"{identity}";

            return this.plainQuestionnairesCache.GetOrAdd(questionnaireCacheKey,
                s => this.translationStorage.Translate(Questionnaire, identity.Version,
                    language));
        }
        
        public void StoreQuestionnaire(Guid id, long version, QuestionnaireDocument questionnaireDocument)
        {
            Questionnaire = new PlainQuestionnaire(questionnaireDocument, version, questionOptionsRepository, substitutionService);
            Questionnaire.ExpressionStorageType = this.prototypeProvider.GetExpressionStorageType(Questionnaire.QuestionnaireIdentity);
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
