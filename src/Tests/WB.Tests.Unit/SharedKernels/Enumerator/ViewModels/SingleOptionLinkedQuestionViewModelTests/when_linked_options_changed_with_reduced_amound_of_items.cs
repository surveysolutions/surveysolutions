using System;
using System.Linq;
using FluentAssertions;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SingleOptionLinkedQuestionViewModelTests
{
    internal class when_linked_options_changed_with_reduced_amound_of_items : SingleOptionLinkedQuestionViewModelTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            linkSourceQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            linkedQuestionId = Create.Entity.Identity(Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), RosterVector.Empty);
            interviewId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC").FormatGuid();
            interviewerId = Guid.Parse("77777777777777777777777777777777");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.SingleOptionQuestion(linkedQuestionId.Id, linkedToQuestionId: linkSourceQuestionId),
                Create.Entity.FixedRoster(fixedTitles: new[] { Create.Entity.FixedTitle(1), Create.Entity.FixedTitle(2), Create.Entity.FixedTitle(3) }, children: new[]
                {
                    Create.Entity.TextQuestion(linkSourceQuestionId)
                }));

            interview = Setup.StatefulInterview(questionnaire);

            interview.AnswerTextQuestion(interviewerId, linkSourceQuestionId, Create.Entity.RosterVector(1), DateTime.UtcNow, "one");
            interview.AnswerTextQuestion(interviewerId, linkSourceQuestionId, Create.Entity.RosterVector(2), DateTime.UtcNow, "two");

            interview.AnswerSingleOptionLinkedQuestion(interviewerId, linkedQuestionId.Id, linkedQuestionId.RosterVector, DateTime.UtcNow, Create.Entity.RosterVector(1));

            viewModel = Create.ViewModel.SingleOptionLinkedQuestionViewModel(interview: interview, questionnaire: Create.Entity.PlainQuestionnaire(questionnaire));
            viewModel.Init(interviewId, linkedQuestionId, Create.Other.NavigationState());

            interview.RemoveAnswer(linkSourceQuestionId, Create.Entity.RosterVector(2), interviewerId, DateTime.UtcNow);
            BecauseOf();
        }

        public void BecauseOf() => viewModel.Handle(Create.Event.LinkedOptionsChanged(new[]
        {
            new ChangedLinkedOptions(linkedQuestionId, new[] { Create.Entity.RosterVector(1) })
        }));


        [NUnit.Framework.Test] public void should_synchronize_visible_options_with_event_data () => viewModel.Options.Count.Should().Be(1);
        [NUnit.Framework.Test] public void should_keep_not_removed_options_as_they_were () => viewModel.Options.First().Title.Should().Be("one");
        [NUnit.Framework.Test] public void should_not_remove_selected_option () => viewModel.Options.First().Selected.Should().BeTrue();

        static SingleOptionLinkedQuestionViewModel viewModel;
        static Guid linkSourceQuestionId;
        static Identity linkedQuestionId;
        static string interviewId;
        static StatefulInterview interview;
        static Guid interviewerId;
    }
}
