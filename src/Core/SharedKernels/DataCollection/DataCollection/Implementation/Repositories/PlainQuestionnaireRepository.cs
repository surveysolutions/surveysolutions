using System;
using Main.Core.Documents;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.PlainRepository;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Repositories
{
    internal class PlainQuestionnaireRepository : IPlainQuestionnaireRepository
    {
        private readonly IPlainRepositoryAccessor<QuestionnaireDocument> repository;

        public PlainQuestionnaireRepository(IPlainRepositoryAccessor<QuestionnaireDocument> repository)
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
            this.repository.Store(GetRepositoryId(id, version), questionnaireDocument);
        }

        private static string GetRepositoryId(Guid id, long version)
        {
            return string.Format("{0}:{1}", id.FormatGuid(), version);
        }
    }
}