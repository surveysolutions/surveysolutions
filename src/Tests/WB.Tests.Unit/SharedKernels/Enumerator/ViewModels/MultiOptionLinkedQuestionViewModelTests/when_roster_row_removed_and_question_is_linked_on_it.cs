using System;
using Machine.Specifications;
using Moq;
using NSubstitute;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.Enumerator.Aggregates;
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

            var questionnaire = Mock.Of<IQuestionnaire>(_
                => _.GetRosterReferencedByLinkedQuestion(questionId.Id) == rosterId.Id
            );

            var interview = Substitute.For<IStatefulInterview>();
            interview.FindReferencedRostersForLinkedQuestion(rosterId.Id, Arg.Any<Identity>())
                 .Returns(new[] {
                     Create.InterviewRoster(rosterId.Id, Create.RosterVector(1), "title"),
                     Create.InterviewRoster(rosterId.Id, Create.RosterVector(2), "title2")
                 });

            viewModel = CreateMultiOptionRosterLinkedQuestionViewModel(questionnaire, interview);
            viewModel.Init("interview", questionId, Create.NavigationState());

            interview.FindReferencedRostersForLinkedQuestion(rosterId.Id, Arg.Any<Identity>())
                   .Returns(new[]
                   {
                       Create.InterviewRoster(rosterId.Id, Create.RosterVector(1), "title")
                   });
        };

        Because of = () =>
        {
            viewModel.Handle(Create.RosterInstancesRemoved(rosterId.Id));
        };

        It should_remove_single_option = () => viewModel.Options.Count.ShouldEqual(1);

        It should_remove_option_with_correct_roster_vector = () => viewModel.Options.ShouldNotContain(o => o.Value.Identical(Create.RosterVector(2)));

        static MultiOptionLinkedToRosterQuestionViewModel viewModel;
        static Identity questionId;
        static Identity rosterId;
    }
}