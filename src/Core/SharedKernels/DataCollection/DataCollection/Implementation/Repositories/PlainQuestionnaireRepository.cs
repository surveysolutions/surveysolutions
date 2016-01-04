using System;
using Main.Core.Documents;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Repositories
{
    internal class PlainQuestionnaireRepository : IPlainQuestionnaireRepository
    {
        private readonly IPlainKeyValueStorage<QuestionnaireDocument> repository;

        public PlainQuestionnaireRepository(IPlainKeyValueStorage<QuestionnaireDocument> repository)
        {
            this.repository = repository;
        }

        public IQuestionnaire GetHistoricalQuestionnaire(Guid id, long version)
        {
            QuestionnaireDocument questionnaireDocument = this.repository.GetById(GetRepositoryId(id, version));

            if (questionnaireDocument == null || questionnaireDocument.IsDeleted)
                return null;

            return new PlainQuestionnaire(questionnaireDocument, version);
        }

        public void StoreQuestionnaire(Guid id, long version, QuestionnaireDocument questionnaireDocument)
        {
            this.repository.Store(questionnaireDocument, GetRepositoryId(id, version));
        }

        public QuestionnaireDocument GetQuestionnaireDocument(Guid id, long version)
        {
            return this.repository.GetById(GetRepositoryId(id, version));
        }

        public QuestionnaireDocument GetQuestionnaireDocument(QuestionnaireIdentity identity)
        {
            return this.GetQuestionnaireDocument(identity.QuestionnaireId, identity.Version);
        }

        public void DeleteQuestionnaireDocument(Guid id, long version)
        {
            var document = GetQuestionnaireDocument(id, version);
            
            if(document==null)
                return;
            document.IsDeleted = true;

            StoreQuestionnaire(id, version, document);
        }

        private static string GetRepositoryId(Guid id, long version)
        {
            return string.Format("{0}${1}", id.FormatGuid(), version);
        }
    }
}