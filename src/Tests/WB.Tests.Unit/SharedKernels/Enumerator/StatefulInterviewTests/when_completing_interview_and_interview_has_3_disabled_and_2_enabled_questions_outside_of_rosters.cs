using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    [Ignore("temp")]
    internal class when_completing_interview_and_interview_has_3_disabled_and_2_enabled_questions_outside_of_rosters
    {
        Establish context = () =>
        {
            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.TextQuestion(questionId: disabledQuestion1Id),
                Create.TextQuestion(questionId: disabledQuestion2Id),
                Create.TextQuestion(questionId: disabledQuestion3Id),
                Create.TextQuestion(questionId: enabledQuestion1Id),
                Create.TextQuestion(questionId: enabledQuestion2Id),
            });

            interview = Setup.StatefulInterview(questionnaireDocument: questionnaire);

            interview.Apply(Create.Event.InterviewStatusChanged(status: InterviewStatus.InterviewerAssigned));

            interview.Apply(Create.Event.QuestionsDisabled(new []
            {
                Create.Identity(disabledQuestion1Id, RosterVector.Empty),
                Create.Identity(disabledQuestion2Id, RosterVector.Empty),
                Create.Identity(disabledQuestion3Id, RosterVector.Empty),
            }));

            eventContext = Create.EventContext();
        };

        Because of = () =>
            interview.Complete(Guid.NewGuid(), string.Empty, DateTime.UtcNow);

        It should_raise_QuestionsDisabled_event = () =>
            eventContext.ShouldContainEvent<QuestionsDisabled>();

        It should_raise_QuestionsDisabled_event_with_ids_of_disabled_questions_and_empty_roster_vectors = () =>
            eventContext.GetEvent<QuestionsDisabled>().Questions.ShouldContainOnly(new[]
            {
                Create.Identity(disabledQuestion1Id, RosterVector.Empty),
                Create.Identity(disabledQuestion2Id, RosterVector.Empty),
                Create.Identity(disabledQuestion3Id, RosterVector.Empty),
            });

        It should_raise_QuestionsEnabled_event = () =>
            eventContext.ShouldContainEvent<QuestionsEnabled>();

        It should_raise_QuestionsEnabled_event_with_ids_of_enabled_questions_and_empty_roster_vectors = () =>
            eventContext.GetEvent<QuestionsEnabled>().Questions.ShouldContainOnly(new[]
            {
                Create.Identity(enabledQuestion1Id, RosterVector.Empty),
                Create.Identity(enabledQuestion2Id, RosterVector.Empty),
            });

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        private static StatefulInterview interview;
        private static EventContext eventContext;
        private static Guid disabledQuestion1Id = Guid.Parse("DDDDDDDDDDDDDDDD1111111111111111");
        private static Guid disabledQuestion2Id = Guid.Parse("DDDDDDDDDDDDDDDD2222222222222222");
        private static Guid disabledQuestion3Id = Guid.Parse("DDDDDDDDDDDDDDDD3333333333333333");
        private static Guid enabledQuestion1Id = Guid.Parse("EEEEEEEEEEEEEEEE1111111111111111");
        private static Guid enabledQuestion2Id = Guid.Parse("EEEEEEEEEEEEEEEE2222222222222222");
    }
}