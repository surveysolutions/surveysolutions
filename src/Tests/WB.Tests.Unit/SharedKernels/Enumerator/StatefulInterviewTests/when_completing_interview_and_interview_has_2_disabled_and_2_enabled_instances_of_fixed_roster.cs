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
    [Ignore("KP-8159")]
    internal class when_completing_interview_and_interview_has_2_disabled_and_2_enabled_instances_of_fixed_roster
    {
        Establish context = () =>
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(chapterId: chapterId, children: new IComposite[]
            {
                Create.Entity.Roster(
                    rosterId: rosterId,
                    fixedRosterTitles: new[]
                    {
                        Create.Entity.FixedTitle(1, "FirstEnabled"),
                        Create.Entity.FixedTitle(2, "SecondEnabled"),
                        Create.Entity.FixedTitle(-3, "ThirdDisabled"),
                        Create.Entity.FixedTitle(-4, "FourthDisabled"),
                    }),
            });

            interview = Setup.StatefulInterview(questionnaireDocument: questionnaire);

            interview.Apply(Create.Event.InterviewStatusChanged(status: InterviewStatus.InterviewerAssigned));

            interview.Apply(Create.Event.RosterInstancesAdded(rosterId, new []
            {
                Create.Entity.RosterVector(1),
                Create.Entity.RosterVector(2),
                Create.Entity.RosterVector(-3),
                Create.Entity.RosterVector(-4),
            }));

            interview.Apply(Create.Event.GroupsDisabled(new []
            {
                Create.Entity.Identity(rosterId, Create.Entity.RosterVector(-3)),
                Create.Entity.Identity(rosterId, Create.Entity.RosterVector(-4)),
            }));

            interview.Apply(Create.Event.GroupsEnabled(new []
            {
                Create.Entity.Identity(rosterId, Create.Entity.RosterVector(1)),
                Create.Entity.Identity(rosterId, Create.Entity.RosterVector(2)),
            }));

            eventContext = Create.Other.EventContext();
        };

        Because of = () =>
            interview.Complete(Guid.NewGuid(), string.Empty, DateTime.UtcNow);

        It should_raise_GroupsDisabled_event = () =>
            eventContext.ShouldContainEvent<GroupsDisabled>();

        It should_raise_GroupsDisabled_event_with_disabled_roster_vectors_of_fixed_roster = () =>
            eventContext.GetEvent<GroupsDisabled>().Groups.ShouldContainOnly(new[]
            {
                Create.Entity.Identity(rosterId, Create.Entity.RosterVector(-3)),
                Create.Entity.Identity(rosterId, Create.Entity.RosterVector(-4)),
            });

        It should_raise_GroupsEnabled_event = () =>
            eventContext.ShouldContainEvent<GroupsEnabled>();

        It should_raise_GroupsEnabled_event_with_enabled_roster_vectors_of_fixed_roster = () =>
            eventContext.GetEvent<GroupsEnabled>().Groups.ShouldContainOnly(new[]
            {
                Create.Entity.Identity(rosterId, Create.Entity.RosterVector(1)),
                Create.Entity.Identity(rosterId, Create.Entity.RosterVector(2)),
            });

        It should_not_raise_QuestionsDisabled_event = () =>
            eventContext.ShouldNotContainEvent<QuestionsDisabled>();

        It should_not_raise_QuestionsEnabled_event = () =>
            eventContext.ShouldNotContainEvent<QuestionsEnabled>();

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        private static StatefulInterview interview;
        private static EventContext eventContext;
        private static Guid chapterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid rosterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
    }
}