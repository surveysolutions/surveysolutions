using System;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    public partial class InterviewTests
    {
        [Test]
	    public void When_answering_on_categorical_single_question_linked_to_list_in_roster_and_text_list_outside_roster_Then_answer_should_be_set()
	    {
	        //arrange
            var textListQuestionId = Guid.Parse("11111111111111111111111111111111");
            var categoricalSingleQuestionId = Guid.Parse("22222222222222222222222222222222");
            var numericRosterSizeId = Guid.Parse("33333333333333333333333333333333");
            var interviewerId = Guid.Parse("44444444444444444444444444444444");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextListQuestion(textListQuestionId),
                Create.Entity.NumericIntegerQuestion(numericRosterSizeId),
                Create.Entity.Roster(rosterSizeSourceType: RosterSizeSourceType.Question,
                    rosterSizeQuestionId: numericRosterSizeId, children: new[]
                    {
                        Create.Entity.SingleOptionQuestion(categoricalSingleQuestionId, linkedToQuestionId: textListQuestionId)
                    }));
            var interview = Setup.StatefulInterview(questionnaire);
            interview.AnswerTextListQuestion(interviewerId, textListQuestionId, RosterVector.Empty, DateTime.UtcNow, new[] {new Tuple<decimal, string>(1, "option 1") });
            interview.AnswerNumericIntegerQuestion(interviewerId, numericRosterSizeId, RosterVector.Empty, DateTime.UtcNow, 1);
            //act
            interview.AnswerSingleOptionQuestion(interviewerId, categoricalSingleQuestionId, Create.Entity.RosterVector(0), DateTime.UtcNow, 1m);
            //assert
            Assert.That(interview.GetSingleOptionLinkedToListQuestion(Identity.Create(categoricalSingleQuestionId,
                Create.Entity.RosterVector(0))).GetAnswer().SelectedValue, Is.EqualTo(1));
	    }
    }
}
