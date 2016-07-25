using System;
using Main.Core.Documents;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Repositories
{
    internal class QuestionnaireStorage : IQuestionnaireStorage
    {
        private readonly IPlainKeyValueStorage<QuestionnaireDocument> repository;
        private readonly ConcurrentDictionary<string, QuestionnaireDocument> cache = new ConcurrentDictionary<string, QuestionnaireDocument>();
        private readonly Dictionary<string, PlainQuestionnaire> plainQuestionnaireCache = new Dictionary<string, PlainQuestionnaire>();
        private readonly ITranslationStorage translationStorage;
        private readonly IQuestionnaireTranslator translator;

        public QuestionnaireStorage(IPlainKeyValueStorage<QuestionnaireDocument> repository, ITranslationStorage translationStorage, IQuestionnaireTranslator translator)
        {
            this.repository = repository;
            this.translationStorage = translationStorage;
            this.translator = translator;
        }

        public IQuestionnaire GetQuestionnaire(QuestionnaireIdentity identity, string language)
        {
            string questionnaireCacheKey = language != null ? $"{identity}${language}" : $"{identity}";

            if (!this.plainQuestionnaireCache.ContainsKey(questionnaireCacheKey))
            {
                QuestionnaireDocument questionnaireDocument = this.GetQuestionnaireDocument(identity.QuestionnaireId, identity.Version);
                if (questionnaireDocument == null || questionnaireDocument.IsDeleted)
                    return null;

                if (language != null)
                {
                    var translationId = questionnaireDocument.Translations.SingleOrDefault(t => t.Name == language)?.Id;

                    var translation = translationId != null ? this.translationStorage.Get(identity, translationId.Value) : null;

                    if (translation == null)
                        throw new ArgumentException($"No translation found for language '{language}' and questionnaire '{identity}'.", nameof(translationId));

                    questionnaireDocument = this.translator.Translate(questionnaireDocument, translation);
                }

                var plainQuestionnaire = new PlainQuestionnaire(questionnaireDocument, identity.Version);
                plainQuestionnaire.WarmUpPriorityCaches();

                this.plainQuestionnaireCache[questionnaireCacheKey] = plainQuestionnaire;
            }

            return this.plainQuestionnaireCache[questionnaireCacheKey];
        }

        public void StoreQuestionnaire(Guid id, long version, QuestionnaireDocument questionnaireDocument)
        {
            string repositoryId = GetRepositoryId(id, version);
            this.repository.Store(questionnaireDocument, repositoryId);
            this.cache[repositoryId] = questionnaireDocument.Clone();
            this.plainQuestionnaireCache.Clear();
        }

        public QuestionnaireDocument GetQuestionnaireDocument(Guid id, long version)
        {
            string repositoryId = GetRepositoryId(id, version);

            if (!this.cache.ContainsKey(repositoryId))
            {
                this.cache[repositoryId] = this.repository.GetById(repositoryId);
            }

            return this.cache[repositoryId];
        }

        public QuestionnaireDocument GetQuestionnaireDocument(QuestionnaireIdentity identity)
        {
            return this.GetQuestionnaireDocument(identity.QuestionnaireId, identity.Version);
        }

        public void DeleteQuestionnaireDocument(Guid id, long version)
        {
            string repositoryId = GetRepositoryId(id, version);
            var document = this.repository.GetById(repositoryId);

            if (document == null)
                return;

            document.IsDeleted = true;
            StoreQuestionnaire(id, version, document);

            this.cache[repositoryId] = null;
            this.plainQuestionnaireCache.Clear();
        }

        private static string GetRepositoryId(Guid id, long version) => $"{id.FormatGuid()}${version}";
    }
}