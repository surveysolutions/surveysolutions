using System;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SingleOptionLinkedQuestionViewModelTests
{
    [Subject(typeof(SingleOptionLinkedQuestionViewModel))]
    internal class SingleOptionLinkedQuestionViewModelTestsContext
    {
        protected static IQuestionnaire SetupQuestionnaireWithSingleOptionQuestionLinkedToTextQuestion(Guid questionId, Guid linkedToQuestionId)
        {
            return Mock.Of<IQuestionnaire>(_ => _.GetQuestionReferencedByLinkedQuestion(questionId) == linkedToQuestionId);
        }
    }
}