using System;
using Main.Core.Documents;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Repositories
{
    internal class PlainQuestionnaireRepository : IPlainQuestionnaireRepository
    {
        private readonly IPlainStorageAccessor<QuestionnaireDocument> repository;

        public PlainQuestionnaireRepository(IPlainStorageAccessor<QuestionnaireDocument> repository)
        {
            this.repository = repository;
        }

        public IQuestionnaire GetQuestionnaire(Guid id)
        {
            throw new NotSupportedException("Plain questionnaire repository should be used only on CAPI and Supervisor and it does not support 'latest questionnaire' method. To create new interview please provide concrete version for creation.");
        }

        public IQuestionnaire GetHistoricalQuestionnaire(Guid id, long version)
        {
            QuestionnaireDocument questionnaireDocument = this.repository.GetById(GetRepositoryId(id, version));

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

        public void DeleteQuestionnaireDocument(Guid id, long version)
        {
            this.repository.Remove(GetRepositoryId(id, version));
        }

        private static string GetRepositoryId(Guid id, long version)
        {
            return string.Format("{0}${1}", id.FormatGuid(), version);
        }
    }
}