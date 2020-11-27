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
        private readonly IInterviewExpressionStatePrototypeProvider expressionStatePrototypeProvider;
        private readonly IMemoryCache memoryCache;

        public QuestionnaireStorage(IPlainKeyValueStorage<QuestionnaireDocument> repository, 
            ITranslationStorage translationStorage, 
            IQuestionnaireTranslator translator,
            IQuestionOptionsRepository questionOptionsRepository,
            ISubstitutionService substitutionService,
            IInterviewExpressionStatePrototypeProvider expressionStatePrototypeProvider,
            IMemoryCache memoryCache
            )
        {
            this.repository = repository;
            this.translationStorage = translationStorage;
            this.translator = translator;
            this.questionOptionsRepository = questionOptionsRepository;
            this.substitutionService = substitutionService;
            this.expressionStatePrototypeProvider = expressionStatePrototypeProvider ?? throw new ArgumentNullException(nameof(expressionStatePrototypeProvider));
            this.memoryCache = memoryCache;
        }

        public virtual IQuestionnaire GetQuestionnaire(QuestionnaireIdentity identity, string language)
        {
            var questionnaireCacheKey = PlainQuestionnaireCacheKey(identity, language);

            return this.memoryCache.GetOrCreate(questionnaireCacheKey, (entry) =>
            {
                entry.SetSlidingExpiration(TimeSpan.FromMinutes(20));
                return CreatePlainQuestionnaire(identity, language);
            });
        }

        private static string PlainQuestionnaireCacheKey(QuestionnaireIdentity identity, string language)
        {
            return language != null ? $"{identity}${language}" : $"{identity}";
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

            var usingExpressionStorage = plainQuestionnaire.IsUsingExpressionStorage();
            if (usingExpressionStorage)
            {
                plainQuestionnaire.ExpressionStorageType = this.expressionStatePrototypeProvider.GetExpressionStorageType(identity);
            }

            return plainQuestionnaire;
        }

        protected virtual QuestionnaireDocument FillPlainQuestionnaireDataOnCreate(QuestionnaireIdentity identity, QuestionnaireDocument questionnaireDocument)
        {
            return questionnaireDocument;
        }

        public virtual void StoreQuestionnaire(Guid id, long version, QuestionnaireDocument questionnaireDocument)
        {
            string repositoryId = GetRepositoryId(new QuestionnaireIdentity(id, version));
            this.repository.Store(questionnaireDocument, repositoryId);

            this.memoryCache.Set(repositoryId, questionnaireDocument.Clone(),
                new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(20)
                });
        }

        public virtual QuestionnaireDocument GetQuestionnaireDocument(Guid id, long version)
        {
            string repositoryId = GetRepositoryId(new QuestionnaireIdentity(id, version));

            return this.memoryCache.GetOrCreate(repositoryId, (entry) =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(20);
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

            this.memoryCache.Remove(repositoryId);
            foreach (var translation in document.Translations)
            {
                this.memoryCache.Remove(PlainQuestionnaireCacheKey(questionnaireIdentity, translation.Name));
            }
            
            this.memoryCache.Remove(PlainQuestionnaireCacheKey(questionnaireIdentity, null));
        }

        protected static string GetRepositoryId(QuestionnaireIdentity questionnaireIdentity)
            => questionnaireIdentity.ToString(); //$"{id.FormatGuid()}${version}";
    }
}
