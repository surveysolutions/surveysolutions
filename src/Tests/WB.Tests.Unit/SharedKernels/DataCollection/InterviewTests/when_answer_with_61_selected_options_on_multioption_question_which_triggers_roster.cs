using System;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answer_with_201_selected_options_on_multioption_question_which_triggers_roster : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            rosterSizeQuestionId = Guid.Parse("33333333333333333333333333333333");

            answers = new int[201];
            for (int i = 0; i < answers.Length; i++)
            {
                answers[i] = i;
            }

            var questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                Create.Entity.MultipleOptionsQuestion(questionId: rosterSizeQuestionId, answers: answers),
                Create.Entity.FixedRoster(children: new[]
                {
                    Create.Entity.FixedRoster(children: new[]
                    {
                        Create.Entity.Roster(
                            rosterId: Guid.Parse("11111111111111111111111111111111"),
                            rosterSizeSourceType: RosterSizeSourceType.Question,
                            rosterSizeQuestionId: rosterSizeQuestionId)
                    })
                })
            );

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                Create.Entity.PlainQuestionnaire(questionnaire, 1));

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
            BecauseOf();
        }

        public void BecauseOf() =>
            exception =  NUnit.Framework.Assert.Throws<AnswerNotAcceptedException>(() => interview.AnswerMultipleOptionsQuestion(userId, rosterSizeQuestionId, new decimal[0], DateTime.Now, answers));

        [NUnit.Framework.Test] public void should_throw_InterviewException () =>
            exception.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_throw_InterviewException_with_explanation () =>
           exception.Message.ToLower().ToSeparateWords().Should().Contain("answer", "'61'", "question", "roster", "greater", "60");

        private static Interview interview;
        private static Guid userId;
        private static Guid rosterSizeQuestionId;
        private static int[] answers;
        private static InterviewException exception;
         
    }
}
