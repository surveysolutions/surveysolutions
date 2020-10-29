using System;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SingleOptionLinkedToListQuestionViewModelTests
{
    [TestOf(typeof(SingleOptionLinkedToListQuestionViewModel))]
    public class when_text_list_with_2_options_and_1_option_removed : SingleOptionLinkedToListQuestionViewModelTests
    {
        [Test]
        public void should_view_model_has_1_option_only()
        {
            //arrange
            var textListQuestionId = Guid.Parse("11111111111111111111111111111111");
            var singleOptionQuestionId = Guid.Parse("22222222222222222222222222222222");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextListQuestion(textListQuestionId),
                Create.Entity.SingleQuestion(singleOptionQuestionId, linkedToQuestionId: textListQuestionId));

            Guid interviewId = Guid.Parse("33333333333333333333333333333333");
            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire, interviewId: interviewId);
            interview.AnswerTextListQuestion(Guid.NewGuid(), textListQuestionId, RosterVector.Empty, DateTime.UtcNow, new[]
            {
                new Tuple<decimal, string>(1, "option 1")
            });

            var eventRegistry = Create.Service.LiteEventRegistry();
            var viewModel = Create.ViewModel.SingleOptionLinkedToListQuestionViewModel(Create.Entity.PlainQuestionnaire(questionnaire), interview, eventRegistry);
            viewModel.Init(interviewId.FormatGuid(), Identity.Create(singleOptionQuestionId, RosterVector.Empty), Create.Other.NavigationState());

            interview.AnswerTextListQuestion(Guid.NewGuid(), textListQuestionId, RosterVector.Empty, DateTime.UtcNow, new Tuple<decimal, string>[0]);

            //act
            viewModel.HandleAsync(Create.Event.LinkedToListOptionsChanged(new[]
            {
                Create.Event.ChangedLinkedToListOptions(Identity.Create(singleOptionQuestionId, RosterVector.Empty))
            }));
            //assert
            Assert.That(viewModel.Options, Is.Empty);
        }

    }
}
