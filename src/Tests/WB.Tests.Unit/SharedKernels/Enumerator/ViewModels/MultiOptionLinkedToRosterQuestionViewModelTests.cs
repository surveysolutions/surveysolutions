using System;
using System.Linq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels
{
    [TestOf(typeof(MultiOptionLinkedToRosterQuestionViewModel))]
    public class MultiOptionLinkedToRosterQuestionViewModelTests
    {
        [Test]
        public void When_section_sections_contains_non_ordered_multi_linked_question_on_roster_Then_options_should_be_without_order_index()
        {
            //arrange
            var numericQuestionId = Guid.Parse("11111111111111111111111111111111");
            var rosterId = Guid.Parse("33333333333333333333333333333333");
            var multiOptionQuestionId = Guid.Parse("22222222222222222222222222222222");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericIntegerQuestion(numericQuestionId),
                Create.Entity.Roster(rosterId, rosterSizeQuestionId: numericQuestionId),
                Create.Entity.MultyOptionsQuestion(multiOptionQuestionId, linkedToRosterId: rosterId, areAnswersOrdered: false));

            var interview = Setup.StatefulInterview(questionnaire);
            interview.AnswerNumericIntegerQuestion(Guid.NewGuid(), numericQuestionId, RosterVector.Empty, DateTime.UtcNow, 5);
            interview.AnswerMultipleOptionsLinkedQuestion(Guid.NewGuid(), multiOptionQuestionId, RosterVector.Empty, DateTime.UtcNow, new RosterVector[]
            {
                new decimal[] { 2 }, 
            });

            var viewModel = Create.ViewModel.MultiOptionLinkedToRosterQuestionViewModel(Create.Entity.PlainQuestionnaire(questionnaire), interview);

            //act
            viewModel.Init(null, Identity.Create(multiOptionQuestionId, RosterVector.Empty), Create.Other.NavigationState());

            //assert
            Assert.That(viewModel.Options.Count, Is.EqualTo(5));
            Assert.That(viewModel.Options.Single(i => i.Checked).CheckedOrder.HasValue, Is.EqualTo(false));
        }
    }
}