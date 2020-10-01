using System;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SingleOptionLinkedQuestionViewModelTests
{
    internal class when_initializing : SingleOptionLinkedQuestionViewModelTestsContext
    {
        [Test]
        public void should_initialise_options()
        {
            Mock<IViewModelEventRegistry> eventRegistryMock = new Mock<IViewModelEventRegistry>();
            NavigationState navigationState = Create.Other.NavigationState();

            Mock<QuestionStateViewModel<SingleOptionLinkedQuestionAnswered>> questionStateMock =
                new Mock<QuestionStateViewModel<SingleOptionLinkedQuestionAnswered>>();

            var linkSourceQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var linkedQuestionId = Create.Entity.Identity(Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), RosterVector.Empty);
            var interviewId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC").FormatGuid();
            var interviewerId = Guid.Parse("77777777777777777777777777777777");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.SingleOptionQuestion(linkedQuestionId.Id, linkedToQuestionId: linkSourceQuestionId),
                Create.Entity.FixedRoster(
                    fixedTitles: new[]
                        {Create.Entity.FixedTitle(1), Create.Entity.FixedTitle(2), Create.Entity.FixedTitle(3)},
                    children: new[]
                    {
                        Create.Entity.TextQuestion(linkSourceQuestionId)
                    }));

            var interview = SetUp.StatefulInterview(questionnaire);

            interview.AnswerTextQuestion(interviewerId, linkSourceQuestionId, Create.Entity.RosterVector(1),
                DateTime.UtcNow, "answer1");
            interview.AnswerTextQuestion(interviewerId, linkSourceQuestionId, Create.Entity.RosterVector(2),
                DateTime.UtcNow, "answer2");


            var viewModel = Create.ViewModel.SingleOptionLinkedQuestionViewModel(
                eventRegistry: eventRegistryMock.Object,
                questionState: questionStateMock.Object,
                questionnaire: Create.Entity.PlainQuestionnaire(questionnaire),
                interview: interview);

            // Act
            viewModel.Init(interviewId, linkedQuestionId, navigationState);

            questionStateMock.Verify(state => state.Init(interviewId, linkedQuestionId, navigationState), Times.Once);
            eventRegistryMock.Verify(registry => registry.Subscribe(viewModel, Moq.It.IsAny<string>()), Times.Once);
            viewModel.Options.Select(option => option.Title).Should().BeEquivalentTo("answer1", "answer2");
        }
    }
}
