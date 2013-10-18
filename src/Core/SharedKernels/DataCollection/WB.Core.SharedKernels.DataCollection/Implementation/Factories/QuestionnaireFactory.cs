using System;
using Main.Core.Documents;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Factories;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Factories
{
    internal class QuestionnaireFactory : IQuestionnaireFactory
    {
        public IQuestionnaire CreateTemporaryInstance(QuestionnaireDocument document)
        {
            return new Questionnaire(Guid.Empty, document);
        }
    }
}