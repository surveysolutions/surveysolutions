using System;
using System.Linq;
using Main.Core.Entities.Composite;
using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SingleOptionLinkedQuestionViewModelTests
{
    internal class when_removing_answer : SingleOptionLinkedQuestionViewModelTestsContext
    {
        [OneTimeSetUp] 
        public void SetUp () {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.SingleOptionQuestion(Id.gA, linkedToQuestionId: Id.gB),
                Create.Entity.FixedRoster(fixedTitles: new[] { Create.Entity.FixedTitle(1), Create.Entity.FixedTitle(2), Create.Entity.FixedTitle(3) }, children: new IComposite[]
                {
                    Create.Entity.TextQuestion(Id.gB)
                }));

            var interview = Setup.StatefulInterview(questionnaire);
            var interviewerId = Guid.Parse("77777777777777777777777777777777");

            interview.AnswerTextQuestion(interviewerId, Id.gB, Create.Entity.RosterVector(1), DateTime.UtcNow, "answer1");
            interview.AnswerTextQuestion(interviewerId, Id.gB, Create.Entity.RosterVector(2), DateTime.UtcNow, "answer2");
            interview.AnswerSingleOptionLinkedQuestion(interviewerId, Id.gA, RosterVector.Empty, DateTimeOffset.UtcNow, Create.RosterVector(1));

            viewModel = Create.ViewModel.SingleOptionLinkedQuestionViewModel(
                Create.Entity.PlainQuestionnaire(questionnaire),
                interview,
                answering: answeringMock.Object);

            viewModel.Init("11111111111111111111111111111111", Create.Entity.Identity(Id.gA, Empty.RosterVector), Create.Other.NavigationState());

            viewModel.Options.First().RemoveAnswerCommand.Execute();
        }

        [Test] 
        public void should_execute_RemoveAnswerCommand_command () =>
            answeringMock.Verify(x => x.SendRemoveAnswerCommandAsync(It.IsAny<RemoveAnswerCommand>()), Times.Once);

        [Test] 
        public void should_clear_selection_from_options () =>
            Assert.That(viewModel.Options.All(x => x.Selected == false), Is.True);

        private static SingleOptionLinkedQuestionViewModel viewModel;
        private static Mock<AnsweringViewModel> answeringMock = new Mock<AnsweringViewModel>();
    }
}
