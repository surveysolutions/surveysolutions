using System;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SingleOptionLinkedToListQuestionViewModelTests
{
    [TestOf(typeof(SingleOptionLinkedToListQuestionViewModel))]
    public class when_text_list_disabled
    {
        [Test]
        public void should_view_model_has_empty_options()
        {
            //arrange
            var textListQuestionId = Guid.Parse("11111111111111111111111111111111");
            var singleOptionQuestionId = Guid.Parse("22222222222222222222222222222222");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextListQuestion(textListQuestionId),
                Create.Entity.SingleOptionQuestion(singleOptionQuestionId, linkedToQuestionId: textListQuestionId));

            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: Create.Entity.PlainQuestionnaire(questionnaire));
            interview.AnswerTextListQuestion(Guid.NewGuid(), textListQuestionId, RosterVector.Empty, DateTime.UtcNow, new []
            {
                new Tuple<decimal, string>(1, "option 1"),
                new Tuple<decimal, string>(2, "option 2"),
                new Tuple<decimal, string>(3, "option 3")
            });
            interview.AnswerSingleOptionQuestion(Guid.NewGuid(), singleOptionQuestionId, RosterVector.Empty, DateTime.UtcNow, 1);

            var viewModel = Create.ViewModel.SingleOptionLinkedToListQuestionViewModel(Create.Entity.PlainQuestionnaire(questionnaire), interview);
            viewModel.Init("interviewid", Identity.Create(singleOptionQuestionId, RosterVector.Empty), Create.Other.NavigationState());
            
            interview.Apply(Create.Event.QuestionsDisabled(textListQuestionId, RosterVector.Empty));

            //act
            viewModel.Handle(Create.Event.QuestionsDisabled(textListQuestionId, RosterVector.Empty));
            //assert
            Assert.That(viewModel.Options, Is.Empty);
        }
        
    }
}