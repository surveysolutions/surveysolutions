using System;
using Main.Core.Documents;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.Repositories
{
    /// <summary>
    /// Interface used to store and read questionnaires in plain way, when no CQRS is involved.
    /// Therefore DTO is fixed and is now QuestionnaireDocument.
    /// </summary>
    public interface IPlainQuestionnaireRepository /*: IPlainQuestionnaireRepository*/
    {
        IQuestionnaire GetHistoricalQuestionnaire(Guid id, long version);
        IQuestionnaire GetQuestionnaire(QuestionnaireIdentity identity);
        void StoreQuestionnaire(Guid id, long version, QuestionnaireDocument questionnaireDocument);

        QuestionnaireDocument GetQuestionnaireDocument(QuestionnaireIdentity identity);
        QuestionnaireDocument GetQuestionnaireDocument(Guid id, long version);
        long GetQuestionnaireContentVersion(QuestionnaireIdentity identity);

        void DeleteQuestionnaireDocument(Guid id, long version);
    }
}