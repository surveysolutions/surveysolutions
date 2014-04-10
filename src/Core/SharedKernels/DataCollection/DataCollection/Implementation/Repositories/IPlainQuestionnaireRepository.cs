using System;
using Main.Core.Documents;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Repositories
{
    /// <summary>
    /// Interface used to store and read questionnaires in plain way, when no CQRS is involved.
    /// Therefore DTO is fixed and is now QuestionnaireDocument.
    /// </summary>
    internal interface IPlainQuestionnaireRepository : IQuestionnaireRepository
    {
        void StoreQuestionnaire(Guid id, long version, QuestionnaireDocument questionnaireDocument);
    }
}