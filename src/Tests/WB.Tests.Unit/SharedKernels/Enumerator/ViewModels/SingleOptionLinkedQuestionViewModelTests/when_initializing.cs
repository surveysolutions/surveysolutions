using System;
using System.Linq;
using FluentAssertions;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SingleOptionLinkedQuestionViewModelTests
{
    internal class when_initializing : SingleOptionLinkedQuestionViewModelTestsContext
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

            interview.AnswerTextQuestion(interviewerId, linkSourceQuestionId, Create.Entity.RosterVector(1), DateTime.UtcNow, "answer1");
            interview.AnswerTextQuestion(interviewerId, linkSourceQuestionId, Create.Entity.RosterVector(2), DateTime.UtcNow, "answer2");


            viewModel = Create.ViewModel.SingleOptionLinkedQuestionViewModel(
                eventRegistry: eventRegistryMock.Object,
                questionState: questionStateMock.Object,
                questionnaire: Create.Entity.PlainQuestionnaire(questionnaire),
                interview: interview);
            BecauseOf();
        }

        public void BecauseOf() =>
            viewModel.Init(interviewId, linkedQuestionId, navigationState);

        [NUnit.Framework.Test] public void should_initialize_question_state () =>
            questionStateMock.Verify(state => state.Init(interviewId, linkedQuestionId, navigationState), Times.Once);

        [NUnit.Framework.Test] public void should_subsribe_self_to_event_registry () =>
            eventRegistryMock.Verify(registry => registry.Subscribe(viewModel, Moq.It.IsAny<string>()), Times.Once);

        [NUnit.Framework.Test] public void should_fill_options_with_answers_from_linked_to_question () =>
            viewModel.Options.Select(option => option.Title).Should().BeEquivalentTo("answer1", "answer2");

        private static Mock<ILiteEventRegistry> eventRegistryMock = new Mock<ILiteEventRegistry>();
        private static SingleOptionLinkedQuestionViewModel viewModel;
        static Guid linkSourceQuestionId;
        static Identity linkedQuestionId;
        static string interviewId;
        static StatefulInterview interview;
        static Guid interviewerId;
        private static NavigationState navigationState = Create.Other.NavigationState();
        private static Mock<QuestionStateViewModel<SingleOptionLinkedQuestionAnswered>> questionStateMock = new Mock<QuestionStateViewModel<SingleOptionLinkedQuestionAnswered>>();
    }
}
