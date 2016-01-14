using System;
using Main.Core.Documents;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.Repositories
{
    /// <summary>
    /// Interface used to store and read questionnaires in plain way, when no CQRS is involved.
    /// Therefore DTO is fixed and is now QuestionnaireDocument.
    /// </summary>
    public interface IPlainQuestionnaireRepository : IQuestionnaireRepository
    {
        void StoreQuestionnaire(Guid id, long version, QuestionnaireDocument questionnaireDocument);

        QuestionnaireDocument GetQuestionnaireDocument(QuestionnaireIdentity identity);
        QuestionnaireDocument GetQuestionnaireDocument(Guid id, long version);

        void DeleteQuestionnaireDocument(Guid id, long version);
    }
}