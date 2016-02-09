using System;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.MultiOptionLinkedQuestionViewModelTests
{
    internal class when_roster_row_removed_and_question_is_linked_on_it : MultiOptionLinkedQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            questionId = new Identity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), Create.RosterVector(1));
            rosterId = new Identity(Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), Create.RosterVector(1));

            var questionnaire = Create.QuestionnaireModel(new BaseQuestionModel[] {
                    new LinkedToRosterMultiOptionQuestionModel()
                    {
                        Id = questionId.Id,
                        LinkedToRosterId = rosterId.Id
                    }
                });

            var interview = new Mock<IStatefulInterview>();
            interview.Setup(x => x.FindReferencedRostersForLinkedQuestion(rosterId.Id, Moq.It.IsAny<Identity>()))
                 .Returns(new[] { Create.InterviewRoster(rosterId.Id, Create.RosterVector(1), "title"), Create.InterviewRoster(rosterId.Id, Create.RosterVector(2), "title2") });

            viewModel = CreateMultiOptionRosterLinkedQuestionViewModel(questionnaire, interview.Object);
            viewModel.Init("interview", questionId, Create.NavigationState());

            interview.Setup(x => x.FindReferencedRostersForLinkedQuestion(rosterId.Id, Moq.It.IsAny<Identity>()))
                   .Returns(new[] { Create.InterviewRoster(rosterId.Id, Create.RosterVector(1), "title")});
        };

        Because of = () =>
        {
            viewModel.Handle(Create.RosterInstancesRemoved(rosterId.Id));
        };

        It should_remove_one_option =
            () => viewModel.Options.ShouldNotContain(o => o.Value.Identical(Create.RosterVector(2)));

        static MultiOptionLinkedToRosterQuestionViewModel viewModel;
        static Identity questionId;
        static Identity rosterId;
    }
}