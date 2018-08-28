using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SingleOptionLinkedToListQuestionViewModelTests
{
    [TestOf(typeof(SingleOptionLinkedToListQuestionViewModel))]
    public class when_answer_removed
    {
        [Test]
        public void should_view_model_has_text_list_options()
        {
            //arrange
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextListQuestion(Id.g1),
                Create.Entity.SingleQuestion(Id.g2, linkedToQuestionId: Id.g1));

            var answeringMock = new Mock<AnsweringViewModel>();

            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
            interview.AnswerTextListQuestion(Guid.NewGuid(), Id.g1, RosterVector.Empty, DateTime.UtcNow, new[]
            {
                new Tuple<decimal, string>(1, "option 1"),
                new Tuple<decimal, string>(2, "option 2"),
                new Tuple<decimal, string>(3, "option 3")
            });
            interview.AnswerSingleOptionQuestion(Guid.NewGuid(), Id.g2, RosterVector.Empty, DateTime.UtcNow, 3);

            var viewModel = Create.ViewModel.SingleOptionLinkedToListQuestionViewModel(
                Create.Entity.PlainQuestionnaire(questionnaire),
                interview,
                answering: answeringMock.Object);
            viewModel.Init("inerviewid", Identity.Create(Id.g2, RosterVector.Empty), Create.Other.NavigationState());

            //act
            viewModel.Options.Last().RemoveAnswerCommand.Execute();

            //assert
            answeringMock.Verify(x => x.SendRemoveAnswerCommandAsync(It.IsAny<RemoveAnswerCommand>()), Times.Once);

            Assert.That(viewModel.Options.All(x => x.Selected == false), Is.True);
        }
    }
}