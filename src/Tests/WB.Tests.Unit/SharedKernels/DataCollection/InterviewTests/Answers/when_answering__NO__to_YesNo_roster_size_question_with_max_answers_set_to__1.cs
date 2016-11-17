using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Answers
{
    internal class when_answering__No__to_YesNo_roster_size_question_with_max_answers_set_to__1 : InterviewTestsContext
    {
        Establish context = () =>
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
        };

        Because of = () => interview.AnswerYesNoQuestion(Create.Command.AnswerYesNoQuestion(
                questionId: rosterTriggerId,
                answeredOptions: new[]
                {
                    Create.Entity.AnsweredYesNoOption(1m, false),
                    Create.Entity.AnsweredYesNoOption(2m, false)
                }));

        It should_allow_no_answers_more_than_max_answers_count = () => eventContext.ShouldContainEvent<YesNoQuestionAnswered>();

        static Interview interview;
        static Guid rosterTriggerId;
        static EventContext eventContext;
    }
}