using System;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.MultiOptionLinkedQuestionViewModelTests
{
    internal class when_answers_removed_event_received_by_linked_on_roster_question : MultiOptionLinkedQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            questionId = new Identity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), new decimal[] { 1 });
            rosterId = new Identity(Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), new decimal[] { 1 });

            var questionnaire = Create.QuestionnaireModel(new BaseQuestionModel[] {
                    new LinkedToRosterMultiOptionQuestionModel()
                    {
                        Id = questionId.Id,
                        LinkedToRosterId = rosterId.Id
                    }
                });

            interview = new Mock<IStatefulInterview>();
            interview.Setup(x => x.FindReferencedRostersForLinkedQuestion(rosterId.Id, Moq.It.IsAny<Identity>()))
                 .Returns(new[] { Create.InterviewRoster(rosterId.Id, new decimal[] { 1 }, "title"), Create.InterviewRoster(rosterId.Id, new decimal[] { 2 }, "title2") });

            var linkedMultiOptionAnswer = new LinkedMultiOptionAnswer(questionId.Id, new decimal[0]);
            linkedMultiOptionAnswer.SetAnswers(new[] {new decimal[] {1}});
            interview.Setup(x => x.GetLinkedMultiOptionAnswer(questionId))
                .Returns(linkedMultiOptionAnswer);

            viewModel = CreateMultiOptionRosterLinkedQuestionViewModel(questionnaire, interview.Object);
            viewModel.Init("interview", questionId, Create.NavigationState());
        };

        Because of = () =>
        {
            viewModel.Handle(Create.Event.AnswersRemoved(questionId));
        };

        It should_uncheck_all_options = () => viewModel.Options.Count(o=>!o.Checked).ShouldEqual(2);

        static MultiOptionLinkedToRosterQuestionViewModel viewModel;
        static Identity questionId;
        static Identity rosterId;
        static Mock<IStatefulInterview> interview;
    }
}