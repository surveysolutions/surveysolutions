using System;
using Main.Core.Documents;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Repositories
{
    internal class PlainQuestionnaireRepositoryWithCache : IPlainQuestionnaireRepository
    {
        private readonly IPlainKeyValueStorage<QuestionnaireDocument> repository;
        private readonly ConcurrentDictionary<string, QuestionnaireDocument> cache = new ConcurrentDictionary<string, QuestionnaireDocument>();
        private readonly Dictionary<QuestionnaireIdentity, PlainQuestionnaire> plainQuestionnaireCache = new Dictionary<QuestionnaireIdentity, PlainQuestionnaire>();
        
        public PlainQuestionnaireRepositoryWithCache(IPlainKeyValueStorage<QuestionnaireDocument> repository)
        {
            this.repository = repository;
        }

        public IQuestionnaire GetHistoricalQuestionnaire(Guid id, long version)
        {
            var identity = new QuestionnaireIdentity(id, version);

            if (!this.plainQuestionnaireCache.ContainsKey(identity))
            {
                QuestionnaireDocument questionnaireDocument = this.GetQuestionnaireDocument(id, version);
                if (questionnaireDocument == null || questionnaireDocument.IsDeleted)
                    return null;
                
                this.plainQuestionnaireCache[identity] = new PlainQuestionnaire(questionnaireDocument, version);
            }

            return this.plainQuestionnaireCache[identity];
        }

        public IQuestionnaire GetQuestionnaire(QuestionnaireIdentity identity)
        {
            return this.GetHistoricalQuestionnaire(identity.QuestionnaireId, identity.Version);
        }

        public void StoreQuestionnaire(Guid id, long version, QuestionnaireDocument questionnaireDocument)
        {
            string repositoryId = GetRepositoryId(id, version);
            this.repository.Store(questionnaireDocument, repositoryId);
            this.cache[repositoryId] = questionnaireDocument;
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
        }

        private static string GetRepositoryId(Guid id, long version)
        {
            return string.Format("{0}${1}", id.FormatGuid(), version);
        }
    }
}