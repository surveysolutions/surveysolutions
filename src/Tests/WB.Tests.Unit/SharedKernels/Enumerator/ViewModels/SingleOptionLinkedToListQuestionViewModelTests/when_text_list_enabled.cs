using System;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SingleOptionLinkedToListQuestionViewModelTests
{
    [TestOf(typeof(SingleOptionLinkedToListQuestionViewModel))]
    public class when_text_list_enabled
    {
        [Test]
        public void should_view_model_has_text_list_options()
        {
            //arrange
            var textListQuestionId = Guid.Parse("11111111111111111111111111111111");
            var singleOptionQuestionId = Guid.Parse("22222222222222222222222222222222");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextListQuestion(textListQuestionId),
                Create.Entity.SingleQuestion(singleOptionQuestionId, linkedToQuestionId: textListQuestionId));

            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
            interview.AnswerTextListQuestion(Guid.NewGuid(), textListQuestionId, RosterVector.Empty, DateTime.UtcNow,
                new[]
                {
                    new Tuple<decimal, string>(1, "option 1"),
                    new Tuple<decimal, string>(2, "option 2"),
                    new Tuple<decimal, string>(3, "option 3")
                });
            interview.AnswerSingleOptionQuestion(Guid.NewGuid(), singleOptionQuestionId, RosterVector.Empty,
                DateTime.UtcNow, 3);
            interview.Apply(Create.Event.QuestionsDisabled(textListQuestionId, RosterVector.Empty));

            var viewModel =
                Create.ViewModel.SingleOptionLinkedToListQuestionViewModel(
                    Create.Entity.PlainQuestionnaire(questionnaire), interview);
            viewModel.Init("inerviewid", Identity.Create(singleOptionQuestionId, RosterVector.Empty),
                Create.Other.NavigationState());

            interview.Apply(Create.Event.QuestionsEnabled(textListQuestionId, RosterVector.Empty));
            //act
            viewModel.Handle(Create.Event.QuestionsEnabled(textListQuestionId, RosterVector.Empty));
            //assert
            Assert.That(viewModel.Options[0].Value, Is.EqualTo(1));
            Assert.That(viewModel.Options[0].Title, Is.EqualTo("option 1"));
            Assert.That(viewModel.Options[0].Selected, Is.False);

            Assert.That(viewModel.Options[1].Value, Is.EqualTo(2));
            Assert.That(viewModel.Options[1].Title, Is.EqualTo("option 2"));
            Assert.That(viewModel.Options[1].Selected, Is.False);

            Assert.That(viewModel.Options[2].Value, Is.EqualTo(3));
            Assert.That(viewModel.Options[2].Title, Is.EqualTo("option 3"));
            Assert.That(viewModel.Options[2].Selected, Is.True);
        }
    }
}
