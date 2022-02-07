using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SingleOptionLinkedToListQuestionViewModelTests
{
    [TestOf(typeof(SingleOptionLinkedToListQuestionViewModel))]
    public class when_recive_event_with_same_answer : SingleOptionLinkedToListQuestionViewModelTests
    {
        [Test]
        public async Task should_dont_send_answer_command()
        {
            //arrange
            var textListQuestionId = Guid.Parse("11111111111111111111111111111111");
            var singleOptionQuestionId = Guid.Parse("22222222222222222222222222222222");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextListQuestion(textListQuestionId),
                Create.Entity.SingleQuestion(singleOptionQuestionId, linkedToQuestionId: textListQuestionId));

            var userId = Guid.NewGuid();
            var interviewId = Guid.Parse("33333333333333333333333333333333");
            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire, interviewId: interviewId);

            interview.AnswerTextListQuestion(userId, textListQuestionId, RosterVector.Empty, DateTime.UtcNow, new[]
            {
                new Tuple<decimal, string>(11, "option 11")
            });
            interview.AnswerSingleOptionQuestion(userId, singleOptionQuestionId, RosterVector.Empty, DateTime.UtcNow, 11);

            var eventRegistry = Create.Service.LiteEventRegistry();
            Mock<AnsweringViewModel> answeringMock = new Mock<AnsweringViewModel>();
            var viewModel = Create.ViewModel.SingleOptionLinkedToListQuestionViewModel(Create.Entity.PlainQuestionnaire(questionnaire), interview, eventRegistry, answering: answeringMock.Object);
            viewModel.Init(interviewId.FormatGuid(), Identity.Create(singleOptionQuestionId, RosterVector.Empty), Create.Other.NavigationState());
            var optionViewModel = Create.ViewModel.SingleOptionQuestionOptionViewModel(value: 11);

            //act
            await viewModel.OptionSelectedAsync(optionViewModel);

            //assert
            Assert.That(viewModel.Options.Single().Value, Is.EqualTo(11));
            answeringMock.Verify(a => a.SendQuestionCommandAsync(It.IsAny<AnswerQuestionCommand>()), Times.Never);
        }
    }
}
