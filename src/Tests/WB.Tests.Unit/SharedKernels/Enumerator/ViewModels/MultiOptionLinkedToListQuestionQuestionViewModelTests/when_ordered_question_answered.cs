using System;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.MultiOptionLinkedToListQuestionQuestionViewModelTests
{
    [TestOf(typeof(MultiOptionLinkedToListQuestionQuestionViewModel))]
    public class when_ordered_question_answered
    {
        [Test]
        public void should_checked_orders_be_updated()
        {
            //arrange
            var textListQuestionId = Guid.Parse("11111111111111111111111111111111");
            var multiOptionQuestionId = Guid.Parse("22222222222222222222222222222222");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextListQuestion(textListQuestionId),
                Create.Entity.MultyOptionsQuestion(multiOptionQuestionId, linkedToQuestionId: textListQuestionId, areAnswersOrdered: true));

            var interview = Setup.StatefulInterview(questionnaire);
            interview.AnswerTextListQuestion(Guid.NewGuid(), textListQuestionId, RosterVector.Empty, DateTime.UtcNow, new []
            {
                new Tuple<decimal, string>(1, "option 1"),
                new Tuple<decimal, string>(2, "option 2"),
                new Tuple<decimal, string>(3, "option 3")
            });

            var viewModel = Create.ViewModel.MultiOptionLinkedToListQuestionQuestionViewModel(Create.Entity.PlainQuestionnaire(questionnaire), interview);
            viewModel.Init(null, Identity.Create(multiOptionQuestionId, RosterVector.Empty), Create.Other.NavigationState());
            //act
            interview.AnswerMultipleOptionsQuestion(Guid.NewGuid(), multiOptionQuestionId, RosterVector.Empty,
                DateTime.UtcNow, new[] {3, 1});
            viewModel.Handle(Create.Event.MultipleOptionsQuestionAnswered(multiOptionQuestionId, RosterVector.Empty, new[] {3m, 1m}));
            //assert
            Assert.That(viewModel.Options[0].CheckedOrder, Is.EqualTo(2));
            Assert.That(viewModel.Options[1].CheckedOrder, Is.Null);
            Assert.That(viewModel.Options[2].CheckedOrder, Is.EqualTo(1));
        }
        
    }
}