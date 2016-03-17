using System;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    [Ignore("temp")]
    internal class when_completing_interview_and_interview_has_3_invalid_and_2_valid_questions_outside_of_rosters
    {
        Establish context = () =>
        {
            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.TextQuestion(questionId: invalidQuestion1Id),
                Create.TextQuestion(questionId: invalidQuestion2Id),
                Create.TextQuestion(questionId: invalidQuestion3Id),
                Create.TextQuestion(questionId: validQuestion1Id),
                Create.TextQuestion(questionId: validQuestion2Id),
            });

            interview = Setup.StatefulInterview(questionnaireDocument: questionnaire);

            interview.Apply(Create.Event.InterviewStatusChanged(status: InterviewStatus.InterviewerAssigned));

            interview.Apply(Create.Event.AnswersDeclaredInvalid(questions: new []
            {
                Create.Identity(invalidQuestion1Id, RosterVector.Empty),
                Create.Identity(invalidQuestion2Id, RosterVector.Empty),
                Create.Identity(invalidQuestion3Id, RosterVector.Empty),
            }));

            eventContext = Create.EventContext();
        };

        Because of = () =>
            interview.Complete(Guid.NewGuid(), string.Empty, DateTime.UtcNow);

        It should_raise_AnswersDeclaredInvalid_event = () =>
            eventContext.ShouldContainEvent<AnswersDeclaredInvalid>();

        It should_raise_AnswersDeclaredInvalid_event_with_ids_of_invalid_questions_and_empty_roster_vectors = () =>
            eventContext.GetEvent<AnswersDeclaredInvalid>().Questions.ShouldContainOnly(new[]
            {
                Create.Identity(invalidQuestion1Id, RosterVector.Empty),
                Create.Identity(invalidQuestion2Id, RosterVector.Empty),
                Create.Identity(invalidQuestion3Id, RosterVector.Empty),
            });

        It should_raise_AnswersDeclaredValid_event = () =>
            eventContext.ShouldContainEvent<AnswersDeclaredValid>();

        It should_raise_AnswersDeclaredValid_event_with_ids_of_valid_questions_and_empty_roster_vectors = () =>
            eventContext.GetEvent<AnswersDeclaredValid>().Questions.ShouldContainOnly(new[]
            {
                Create.Identity(validQuestion1Id, RosterVector.Empty),
                Create.Identity(validQuestion2Id, RosterVector.Empty),
            });

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        private static StatefulInterview interview;
        private static EventContext eventContext;
        private static Guid invalidQuestion1Id = Guid.Parse("BBBBBBBBBBBBBBBB1111111111111111");
        private static Guid invalidQuestion2Id = Guid.Parse("BBBBBBBBBBBBBBBB2222222222222222");
        private static Guid invalidQuestion3Id = Guid.Parse("BBBBBBBBBBBBBBBB3333333333333333");
        private static Guid validQuestion1Id = Guid.Parse("CCCCCCCCCCCCCCCC1111111111111111");
        private static Guid validQuestion2Id = Guid.Parse("CCCCCCCCCCCCCCCC2222222222222222");
    }
}