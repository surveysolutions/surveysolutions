using System;
using Main.Core.Documents;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Repositories
{
    public class QuestionnaireStorage : IQuestionnaireStorage
    {
        protected readonly IPlainKeyValueStorage<QuestionnaireDocument> repository;
        private readonly ITranslationStorage translationStorage;
        private readonly IQuestionnaireTranslator translator;
        private readonly IQuestionOptionsRepository questionOptionsRepository;
        private readonly ISubstitutionService substitutionService;
        private readonly IInterviewExpressionStorageProvider expressionStorageProvider;
        private readonly IMemoryCache memoryCache;

        private static readonly TimeSpan QuestionnaireDocumentExpiration = TimeSpan.FromMinutes(5);

        public QuestionnaireStorage(IPlainKeyValueStorage<QuestionnaireDocument> repository,
            ITranslationStorage translationStorage,
            IQuestionnaireTranslator translator,
            IQuestionOptionsRepository questionOptionsRepository,
            ISubstitutionService substitutionService,
            IInterviewExpressionStorageProvider expressionStorageProvider,
            IMemoryCache memoryCache)
        {
            this.repository = repository;
            this.translationStorage = translationStorage;
            this.translator = translator;
            this.questionOptionsRepository = questionOptionsRepository;
            this.substitutionService = substitutionService;
            this.expressionStorageProvider = expressionStorageProvider ?? throw new ArgumentNullException(nameof(expressionStorageProvider));
            this.memoryCache = memoryCache;
        }

        public virtual IQuestionnaire GetQuestionnaire(QuestionnaireIdentity identity, string language)
        {
            var questionnaireCacheKey = PlainQuestionnaireCacheKey(identity, language);

            var plainQuestionnaire = this.memoryCache.GetOrCreate(questionnaireCacheKey, (entry) =>
            {
                entry.SetSlidingExpiration(QuestionnaireDocumentExpiration);
                return CreatePlainQuestionnaire(identity, language);
            });
            if (plainQuestionnaire != null)
                plainQuestionnaire.QuestionOptionsRepository = questionOptionsRepository;
            return plainQuestionnaire;
        }

        private static string PlainQuestionnaireCacheKey(QuestionnaireIdentity identity, string language = null)
        {
            return language != null ? $"qs:{identity}${language}" : $"qs:{identity}";
        }

        public IQuestionnaire GetQuestionnaireOrThrow(QuestionnaireIdentity identity, string language)
        {
            return this.GetQuestionnaire(identity, language) ?? throw new QuestionnaireException("Questionnaire not found");
        }

        private PlainQuestionnaire CreatePlainQuestionnaire(QuestionnaireIdentity identity, string language)
        {
            QuestionnaireDocument questionnaireDocument = this.GetQuestionnaireDocument(identity.QuestionnaireId, identity.Version);
            if (questionnaireDocument == null || questionnaireDocument.IsDeleted)
                return null;

            questionnaireDocument = FillPlainQuestionnaireDataOnCreate(identity, questionnaireDocument);

            Translation translationId = null;
            if (language != null)
            {
                translationId = questionnaireDocument.Translations.SingleOrDefault(t => t.Name == language);

                var translation = translationId != null ? this.translationStorage.Get(identity, translationId.Id) : null;

                if (translation == null)
                    throw new ArgumentException($"No translation found for language '{language}' and questionnaire '{identity}'.", nameof(translationId));

                questionnaireDocument = this.translator.Translate(questionnaireDocument, translation);
            }

            var plainQuestionnaire = new PlainQuestionnaire(questionnaireDocument, identity.Version,
                questionOptionsRepository, substitutionService, translationId);

            plainQuestionnaire.WarmUpPriorityCaches();
            plainQuestionnaire.ExpressionStorageType = this.expressionStorageProvider.GetExpressionStorageType(identity);

            return plainQuestionnaire;
        }

        protected virtual QuestionnaireDocument FillPlainQuestionnaireDataOnCreate(QuestionnaireIdentity identity, QuestionnaireDocument questionnaireDocument)
        {
            return questionnaireDocument;
        }

        public virtual void StoreQuestionnaire(Guid id, long version, QuestionnaireDocument questionnaireDocument)
        {
            var identity = new QuestionnaireIdentity(id, version);
            string repositoryId = GetRepositoryId(identity);
            this.repository.Store(questionnaireDocument, repositoryId);

            if (questionnaireDocument.IsDeleted)
            {
                RemoveDocumentFromCache(identity, questionnaireDocument);
            }
            else
            {
                this.memoryCache.Set(GetCacheKey(identity), questionnaireDocument.Clone(),
                    new MemoryCacheEntryOptions
                    {
                        SlidingExpiration = QuestionnaireDocumentExpiration
                    });
                this.memoryCache.Remove(PlainQuestionnaireCacheKey(identity));
            }
        }

        public virtual QuestionnaireDocument GetQuestionnaireDocument(Guid id, long version)
        {
            var identity = new QuestionnaireIdentity(id, version);
            string repositoryId = GetRepositoryId(identity);

            return this.memoryCache.GetOrCreate(GetCacheKey(identity), (entry) =>
            {
                entry.SlidingExpiration = QuestionnaireDocumentExpiration;
                return this.repository.GetById(repositoryId);
            });
        }

        public QuestionnaireDocument GetQuestionnaireDocument(QuestionnaireIdentity identity)
        {
            return this.GetQuestionnaireDocument(identity.QuestionnaireId, identity.Version);
        }

        public void DeleteQuestionnaireDocument(Guid id, long version)
        {
            var questionnaireIdentity = new QuestionnaireIdentity(id, version);

            string repositoryId = GetRepositoryId(questionnaireIdentity);
            var document = this.repository.GetById(repositoryId);

            if (document == null)
                return;

            document.IsDeleted = true;
            StoreQuestionnaire(id, version, document);
        }

        private void RemoveDocumentFromCache(QuestionnaireIdentity questionnaireIdentity, QuestionnaireDocument document)
        {
            this.memoryCache.Remove(GetCacheKey(questionnaireIdentity));
            foreach (var translation in document.Translations)
            {
                this.memoryCache.Remove(PlainQuestionnaireCacheKey(questionnaireIdentity, translation.Name));
            }

            this.memoryCache.Remove(PlainQuestionnaireCacheKey(questionnaireIdentity, null));
        }

        protected static string GetCacheKey(QuestionnaireIdentity questionnaireIdentity)
            => "qdoc::" + questionnaireIdentity.ToString();

        protected static string GetRepositoryId(QuestionnaireIdentity questionnaireIdentity)
            => questionnaireIdentity.ToString(); //$"{id.FormatGuid()}${version}";
    }
}
