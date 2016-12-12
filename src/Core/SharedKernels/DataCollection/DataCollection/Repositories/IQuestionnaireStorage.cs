using System;
using Main.Core.Documents;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.Repositories
{
    public interface IQuestionnaireStorage
    {
        IQuestionnaire GetQuestionnaire(QuestionnaireIdentity identity, string language);
        
        void StoreQuestionnaire(Guid id, long version, QuestionnaireDocument questionnaireDocument);

        QuestionnaireDocument GetQuestionnaireDocument(QuestionnaireIdentity identity);
        QuestionnaireDocument GetQuestionnaireDocument(Guid id, long version);

        void DeleteQuestionnaireDocument(Guid id, long version);
    }
}