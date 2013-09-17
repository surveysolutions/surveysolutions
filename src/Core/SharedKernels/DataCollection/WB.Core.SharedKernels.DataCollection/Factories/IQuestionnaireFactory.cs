using System;
using Main.Core.Documents;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.SharedKernels.DataCollection.Factories
{
    public interface IQuestionnaireFactory
    {
        IQuestionnaire CreateTemporaryInstance(QuestionnaireDocument document);
    }
}