using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answer_with_11_selected_options_on_multioption_question_which_triggers_roster_and_has_maxAllowedAnswers_10 : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            rosterSizeQuestionId = Guid.Parse("33333333333333333333333333333333");

            answers = Enumerable.Range(1, 11).ToArray();

            var questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                Create.Entity.MultipleOptionsQuestion(questionId: rosterSizeQuestionId, answers: answers, maxAllowedAnswers: 10),
                Create.Entity.Roster(rosterId: Guid.Parse("11111111111111111111111111111111"),
                    rosterSizeSourceType: RosterSizeSourceType.Question, rosterSizeQuestionId: rosterSizeQuestionId));

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                Create.Entity.PlainQuestionnaire(questionnaire, 1));

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
        };

        Because of = () =>
            exception = Catch.Only<InterviewException>(() => interview.AnswerMultipleOptionsQuestion(userId, rosterSizeQuestionId, new decimal[0], DateTime.Now, answers));

        It should_throw_InterviewException = () =>
            exception.ShouldNotBeNull();

        It should_throw_InterviewException_with_explanation = () =>
            exception.Message.ToLower().ToSeparateWords().ShouldContain("11", "answers", "greater", "maximum", "10");

        private static Interview interview;
        private static Guid userId;
        private static Guid rosterSizeQuestionId;
        private static int[] answers;
        private static InterviewException exception;

    }
}