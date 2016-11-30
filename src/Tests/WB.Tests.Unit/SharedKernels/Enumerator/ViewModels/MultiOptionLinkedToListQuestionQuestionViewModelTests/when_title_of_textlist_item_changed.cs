using System;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.MultiOptionLinkedToListQuestionQuestionViewModelTests
{
    [TestOf(typeof(MultiOptionLinkedToListQuestionQuestionViewModel))]
    public class when_title_of_textlist_item_changed
    {
        [Test]
        public void should_title_of_option_be_updated()
        {
            //arrange
            string expectedTitle = "new title";
            var textListQuestionId = Guid.Parse("11111111111111111111111111111111");
            var multiOptionQuestionId = Guid.Parse("22222222222222222222222222222222");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextListQuestion(textListQuestionId),
                Create.Entity.MultyOptionsQuestion(multiOptionQuestionId, linkedToQuestionId: textListQuestionId));

            var interview = Setup.StatefulInterview(questionnaire);
            interview.AnswerTextListQuestion(Guid.NewGuid(), textListQuestionId, RosterVector.Empty, DateTime.UtcNow, new []
            {
                new Tuple<decimal, string>(1, "old title") 
            });

            var viewModel = Create.ViewModel.MultiOptionLinkedToListQuestionQuestionViewModel(Create.Entity.PlainQuestionnaire(questionnaire), interview);
            viewModel.Init(null, Identity.Create(multiOptionQuestionId, RosterVector.Empty), Create.Other.NavigationState());

            var answers = new[] {new Tuple<decimal, string>(1, expectedTitle)};

            interview.AnswerTextListQuestion(Guid.NewGuid(), textListQuestionId, RosterVector.Empty, DateTime.UtcNow, answers);
            //act
            viewModel.Handle(Create.Event.TextListQuestionAnswered(textListQuestionId, Create.Entity.RosterVector(), answers: answers));
            //assert
            Assert.That(viewModel.Options[0].Title, Is.EqualTo(expectedTitle));
        }
        
    }
}