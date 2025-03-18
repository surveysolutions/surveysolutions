using System;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answer_with_11_selected_options_on_multioption_question_which_triggers_roster_and_has_maxAllowedAnswers_10 : InterviewTestsContext
    {
        [NUnit.Framework.Test] public void should_throw_InterviewException_with_explanation () {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            var userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            var rosterSizeQuestionId = Guid.Parse("33333333333333333333333333333333");

            var answers = Enumerable.Range(1, 11).ToArray();

            var questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                Create.Entity.MultipleOptionsQuestion(questionId: rosterSizeQuestionId, answers: answers, maxAllowedAnswers: 10),
                Create.Entity.Roster(rosterId: Guid.Parse("11111111111111111111111111111111"),
                    rosterSizeSourceType: RosterSizeSourceType.Question, rosterSizeQuestionId: rosterSizeQuestionId));

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                Create.Entity.PlainQuestionnaire(questionnaire, 1));

            var interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

            // Act
            var exception =  NUnit.Framework.Assert.Throws<AnswerNotAcceptedException>(() => interview.AnswerMultipleOptionsQuestion(userId, rosterSizeQuestionId, new decimal[0], DateTime.Now, answers));

            // Assert
            exception.Should().NotBeNull();
            exception.Message.Should().Be("Number of answers is greater than the allowed maximum number of selected answers");
        }
    }
}
