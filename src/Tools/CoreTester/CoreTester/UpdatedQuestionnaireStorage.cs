using System;
using Main.Core.Documents;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace CoreTester
{
    public interface IUpdatedQuestionnaireStorage : IQuestionnaireStorage
    {
    }

    public class UpdatedQuestionnaireStorage : IUpdatedQuestionnaireStorage
    {
        public IQuestionnaire GetQuestionnaire(QuestionnaireIdentity identity, string language)
        {
            throw new NotImplementedException();
        }

        public void StoreQuestionnaire(Guid id, long version, QuestionnaireDocument questionnaireDocument)
        {
            throw new NotImplementedException();
        }

        public QuestionnaireDocument GetQuestionnaireDocument(QuestionnaireIdentity identity)
        {
            throw new NotImplementedException();
        }

        public QuestionnaireDocument GetQuestionnaireDocument(Guid id, long version)
        {
            throw new NotImplementedException();
        }

        public void DeleteQuestionnaireDocument(Guid id, long version)
        {
            throw new NotImplementedException();
        }
    }
}