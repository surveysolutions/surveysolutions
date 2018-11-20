using System;
using Main.Core.Documents;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using System.Collections.Concurrent;
using System.Linq;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Repositories
{
    public class QuestionnaireStorage : IQuestionnaireStorage
    {
        private readonly IPlainKeyValueStorage<QuestionnaireDocument> repository;
        private readonly ITranslationStorage translationStorage;
        private readonly IQuestionnaireTranslator translator;

        private static ConcurrentDictionary<string, QuestionnaireDocument> questionnaireDocumentsCache = new ConcurrentDictionary<string, QuestionnaireDocument>();
        private static ConcurrentDictionary<string, PlainQuestionnaire> plainQuestionnairesCache = new ConcurrentDictionary<string, PlainQuestionnaire>();

        private ISubstitutionService substitutionService;

        public QuestionnaireStorage(IPlainKeyValueStorage<QuestionnaireDocument> repository, 
            ITranslationStorage translationStorage, 
            IQuestionnaireTranslator translator,
            ISubstitutionService substitutionService)
        {
            this.repository = repository;
            this.translationStorage = translationStorage;
            this.translator = translator;
            this.substitutionService = substitutionService;
        }

        public IQuestionnaire GetQuestionnaire(QuestionnaireIdentity identity, string language)
        {
            string questionnaireCacheKey = language != null ? $"{identity}${language}" : $"{identity}";

            return plainQuestionnairesCache.GetOrAdd(questionnaireCacheKey, s => CreatePlainQuestionnaire(identity, language));
        }

        private PlainQuestionnaire CreatePlainQuestionnaire(QuestionnaireIdentity identity, string language)
        {
            QuestionnaireDocument questionnaireDocument = this.GetQuestionnaireDocument(identity.QuestionnaireId, identity.Version);
            if (questionnaireDocument == null || questionnaireDocument.IsDeleted)
                return null;

            Translation translationId = null;
            if (language != null)
            {
                translationId = questionnaireDocument.Translations.SingleOrDefault(t => t.Name == language);

                var translation = translationId != null ? this.translationStorage.Get(identity, translationId.Id) : null;

                if (translation == null)
                    throw new ArgumentException($"No translation found for language '{language}' and questionnaire '{identity}'.", nameof(translationId));

                questionnaireDocument = this.translator.Translate(questionnaireDocument, translation);
            }

            var plainQuestionnaire = new PlainQuestionnaire(questionnaireDocument, identity.Version, substitutionService,translationId);

            plainQuestionnaire.WarmUpPriorityCaches();

            return plainQuestionnaire;
        }

        public virtual void StoreQuestionnaire(Guid id, long version, QuestionnaireDocument questionnaireDocument)
        {
            string repositoryId = GetRepositoryId(new QuestionnaireIdentity(id, version));
            this.repository.Store(questionnaireDocument, repositoryId);
            questionnaireDocumentsCache[repositoryId] = questionnaireDocument.Clone();
            plainQuestionnairesCache.Clear();
        }

        public QuestionnaireDocument GetQuestionnaireDocument(Guid id, long version)
        {
            string repositoryId = GetRepositoryId(new QuestionnaireIdentity(id, version));

            if (!questionnaireDocumentsCache.ContainsKey(repositoryId))
            {
                var questionnaire = this.repository.GetById(repositoryId);

                if (questionnaire == null)
                {
                    return null;
                }

                questionnaireDocumentsCache[repositoryId] = questionnaire;
                //  ?? throw new ApplicationException($"Questionnaire {repositoryId} was not found");
            }

            return questionnaireDocumentsCache[repositoryId];
        }

        public QuestionnaireDocument GetQuestionnaireDocument(QuestionnaireIdentity identity)
        {
            return this.GetQuestionnaireDocument(identity.QuestionnaireId, identity.Version);
        }

        public void DeleteQuestionnaireDocument(Guid id, long version)
        {
            string repositoryId = GetRepositoryId(new QuestionnaireIdentity(id, version));
            var document = this.repository.GetById(repositoryId);

            if (document == null)
                return;

            document.IsDeleted = true;
            StoreQuestionnaire(id, version, document);

            questionnaireDocumentsCache.TryRemove(repositoryId, out _);
            plainQuestionnairesCache.Clear();
        }

        private static string GetRepositoryId(QuestionnaireIdentity questionnaireIdentity)
            => questionnaireIdentity.ToString(); //$"{id.FormatGuid()}${version}";
    }
}
