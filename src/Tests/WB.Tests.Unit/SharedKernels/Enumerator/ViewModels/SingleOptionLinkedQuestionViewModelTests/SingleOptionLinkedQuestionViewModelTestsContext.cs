using System;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SingleOptionLinkedQuestionViewModelTests
{
    [Subject(typeof(SingleOptionLinkedQuestionViewModel))]
    internal class SingleOptionLinkedQuestionViewModelTestsContext
    {
        protected static QuestionnaireDocument SetupQuestionnaireWithSingleOptionQuestionLinkedToTextQuestion(Guid questionId, Guid linkedToQuestionId)
            => Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.SingleOptionQuestion(questionId, linkedToQuestionId: linkedToQuestionId),
                Create.Entity.FixedRoster(fixedRosterTitles: new[]
                {
                    new FixedRosterTitle(1, "fixed title 1"),
                    new FixedRosterTitle(2, "fixed title 2"),
                    new FixedRosterTitle(3, "fixed title 3")
                }));
    }
}