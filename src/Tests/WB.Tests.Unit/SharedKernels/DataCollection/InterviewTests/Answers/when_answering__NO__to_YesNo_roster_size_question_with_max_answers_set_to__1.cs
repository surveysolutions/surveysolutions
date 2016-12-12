using System;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Answers
{
    [TestFixture, TestOf(typeof(Interview))]
    internal class when_answering__No__to_YesNo_roster_size_question_with_max_answers_set_to__1 : InterviewTestsContext
    {
        [OneTimeSetUp]
        public void context()
        {
            rosterTriggerId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var rosterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var questionnaireId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                null,
                questionnaireId,
                Create.Entity.YesNoQuestion(
                    questionId: rosterTriggerId,
                    answers: new[] { 1, 2 },
                    maxAnswersCount: 1
                    ),
                Create.Entity.Roster(rosterId: rosterId,
                    rosterSizeSourceType: RosterSizeSourceType.Question,
                    rosterSizeQuestionId: rosterTriggerId));

            var plainQuestionnaire = Create.Entity.PlainQuestionnaire(questionnaire);

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                plainQuestionnaire);

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

            eventContext = new EventContext();

            // Act
            interview.AnswerYesNoQuestion(Create.Command.AnswerYesNoQuestion(
                questionId: rosterTriggerId,
                answeredOptions: new[]
                {
                    Create.Entity.AnsweredYesNoOption(1m, false),
                    Create.Entity.AnsweredYesNoOption(2m, false)
                }));
        }

        [Test]
        public void should_allow_no_answers_more_than_max_answers_count()
        {
            eventContext.ShouldContainEvent<YesNoQuestionAnswered>();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            eventContext.Dispose();
        }

        static Interview interview;
        static Guid rosterTriggerId;
        static EventContext eventContext;
    }
}