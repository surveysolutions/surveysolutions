using System;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_completing_interview_and_interview_has_2_disabled_and_2_enabled_instances_of_fixed_roster
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
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

            interview = SetUp.StatefulInterview(questionnaireDocument: questionnaire);
            
            interview.Apply(Create.Event.GroupsDisabled(new []
            {
                Create.Entity.Identity(rosterId, Create.Entity.RosterVector(-3)),
                Create.Entity.Identity(rosterId, Create.Entity.RosterVector(-4)),
            }));

            eventContext = new EventContext();
            BecauseOf();
        }

        private void BecauseOf() =>
            interview.Complete(Guid.NewGuid(), string.Empty, DateTime.UtcNow, null);

        [NUnit.Framework.Test] public void should_raise_GroupsDisabled_event () =>
            eventContext.ShouldContainEvent<GroupsDisabled>();

        [NUnit.Framework.Test] public void should_raise_GroupsDisabled_event_with_disabled_roster_vectors_of_fixed_roster () =>
            eventContext.GetEvent<GroupsDisabled>().Groups.Should().BeEquivalentTo(new[]
            {
                Create.Entity.Identity(rosterId, Create.Entity.RosterVector(-3)),
                Create.Entity.Identity(rosterId, Create.Entity.RosterVector(-4)),
            });

        [NUnit.Framework.Test] public void should_not_raise_QuestionsDisabled_event () =>
            eventContext.ShouldNotContainEvent<QuestionsDisabled>();

        [NUnit.Framework.Test] public void should_not_raise_QuestionsEnabled_event () =>
            eventContext.ShouldNotContainEvent<QuestionsEnabled>();

        private static StatefulInterview interview;
        private static EventContext eventContext;
        private static Guid chapterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid rosterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
    }
}
