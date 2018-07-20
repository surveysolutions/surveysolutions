using System;
using FluentAssertions;
using Ncqrs.Spec;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_synchronizing_interview_events_for_interviews_created_before_version_5_15 : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            eventContext = new EventContext();
            var questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(
                Create.Entity.QuestionnaireIdentity(questionnaireId, 1), 
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextListQuestion(questionId),
                    Create.Entity.ListRoster(rosterId, rosterSizeQuestionId: questionId)
                    ));

            interview = Create.AggregateRoot.StatefulInterview(questionnaireRepository: questionnaireRepository);
            interview.Apply(new InterviewStatusChanged(InterviewStatus.InterviewerAssigned, "", DateTimeOffset.Now));
            interview.Apply(new InterviewerAssigned(userId, userId, DateTime.Now));

            command = Create.Command.SynchronizeInterviewEventsCommand(
                       userId: userId,
                       questionnaireId: questionnaireId,
                       questionnaireVersion: 1,
                       synchronizedEvents: new IEvent[]
                       {
                           Create.Event.RosterInstancesAdded(rosterId, Create.Entity.RosterVector(1)),
                           Create.Event.RosterInstancesTitleChanged(rosterId, Create.Entity.RosterVector(1), "Hello", RosterVector.Empty, 1),
                           Create.Event.TextListQuestionAnswered(questionId, RosterVector.Empty, new []{ new Tuple<decimal, string>(1, "Hello") }),
                           Create.Event.RosterInstancesAdded(rosterId, Create.Entity.RosterVector(2)),
                           Create.Event.RosterInstancesTitleChanged(rosterId, Create.Entity.RosterVector(2), "World", RosterVector.Empty, 2),
                           Create.Event.TextListQuestionAnswered(questionId, RosterVector.Empty, new []{ new Tuple<decimal, string>(1, "Hello"), new Tuple<decimal, string>(2, "World") })
                       },
                       createdOnClient: false);
            BecauseOf();
        }

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            eventContext.Dispose();
            eventContext = null;
        }

        public void BecauseOf() => interview.SynchronizeInterviewEvents(command);

        [NUnit.Framework.Test] public void should_set__Hello__as_first_roster_title () =>
            interview.GetRosterTitle(Create.Entity.Identity(rosterId, Create.Entity.RosterVector(1))).Should().Be("Hello");

        [NUnit.Framework.Test] public void should_set__World__as_second_roster_title () =>
            interview.GetRosterTitle(Create.Entity.Identity(rosterId, Create.Entity.RosterVector(2))).Should().Be("World");

        static EventContext eventContext;
        static readonly Guid questionnaireId = Guid.Parse("10000000000000000000000000000000");
        static readonly Guid userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        static readonly Guid questionId = Guid.Parse("11111111111111111111111111111111");
        static readonly Guid rosterId =   Guid.Parse("22222222222222222222222222222222");

        static StatefulInterview interview;
        static SynchronizeInterviewEventsCommand command;
    }
}
