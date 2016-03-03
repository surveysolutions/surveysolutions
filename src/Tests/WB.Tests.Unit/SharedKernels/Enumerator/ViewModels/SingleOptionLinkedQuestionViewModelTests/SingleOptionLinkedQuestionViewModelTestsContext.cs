using System;
using Moq;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SingleOptionLinkedQuestionViewModelTests
{
    internal class SingleOptionLinkedQuestionViewModelTestsContext
    {
        protected static IQuestionnaire SetupQuestionnaireWithSingleOptionQuestionLinkedToTextQuestion(Guid questionId, Guid linkedToQuestionId)
        {
            return Mock.Of<IQuestionnaire>(_ => _.GetQuestionReferencedByLinkedQuestion(questionId) == linkedToQuestionId);
        }
    }
}