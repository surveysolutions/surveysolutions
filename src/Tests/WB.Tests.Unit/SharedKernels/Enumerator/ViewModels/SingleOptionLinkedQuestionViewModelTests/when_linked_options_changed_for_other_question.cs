using System;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SingleOptionLinkedQuestionViewModelTests
{
    internal class when_linked_options_changed_for_other_question : SingleOptionLinkedQuestionViewModelTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            linkSourceQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            linkedQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            interviewId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC").FormatGuid();
            var interviewerId = Guid.Parse("77777777777777777777777777777777");

            eventData = new[]
            {
                new ChangedLinkedOptions(Create.Entity.Identity(Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD"), RosterVector.Empty),
                    new[]
                    {
                        Create.Entity.RosterVector(1)
                    }),
            };

            linkedOptionTextInInterview = "answer in init";

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.SingleOptionQuestion(linkedQuestionId, linkedToQuestionId: linkSourceQuestionId),
                Create.Entity.FixedRoster(fixedTitles: new[] { Create.Entity.FixedTitle(1), Create.Entity.FixedTitle(2), Create.Entity.FixedTitle(3) }, children: new[]
                {
                    Create.Entity.TextQuestion(linkSourceQuestionId)
                }));

            StatefulInterview interview = Setup.StatefulInterview(questionnaire);

            interview.AnswerTextQuestion(interviewerId, linkSourceQuestionId, Create.Entity.RosterVector(1), DateTime.UtcNow, linkedOptionTextInInterview);

            viewModel = Create.ViewModel.SingleOptionLinkedQuestionViewModel(interview: interview, questionnaire: Create.Entity.PlainQuestionnaire(questionnaire));
            viewModel.Init(interviewId, Identity.Create(linkedQuestionId, RosterVector.Empty), Create.Other.NavigationState());

            BecauseOf();
        }

        public void BecauseOf() => viewModel.Handle(Create.Event.LinkedOptionsChanged(eventData));

        [NUnit.Framework.Test] public void should_not_modify_list_of_options () => viewModel.Options.Count.Should().Be(1);

        [NUnit.Framework.Test] public void should_not_modify_option_title () => viewModel.Options.First().Title.Should().Be(linkedOptionTextInInterview);

        static SingleOptionLinkedQuestionViewModel viewModel;
        static Guid linkSourceQuestionId;
        static Guid linkedQuestionId;
        static string interviewId;
        static ChangedLinkedOptions[] eventData;
        static string linkedOptionTextInInterview;
    }
}
