using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    [TestFixture]
    internal class when_creating_interview_and_questionnaire_has_fixed_roster_with_3_fixed_titles : InterviewTestsContext
    {
        [OneTimeSetUp]
        public void context()
        {
            questionnaireId = Guid.Parse("22220000000000000000000000000000");
            userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            supervisorId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var answersToFeaturedQuestions = new List<InterviewAnswer>();
            answersTime = new DateTime(2013, 09, 01);
            fixedRosterId = Guid.Parse("a7b0d842-0355-4eab-a943-968c9c013d97");

            var questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(
                Create.Entity.QuestionnaireIdentity(questionnaireId, 1),
                Create.Entity.QuestionnaireDocumentWithOneChapter(id: questionnaireId, children: new IComposite[]
                {
                    Create.Entity.Roster(rosterId: fixedRosterId, variable: "rosterFixed",
                        fixedRosterTitles: new[]
                        {
                            new FixedRosterTitle(0, "Title 1"),
                            new FixedRosterTitle(1, "Title 2"),
                            new FixedRosterTitle(2, "Title 3")
                        },
                        rosterSizeSourceType: RosterSizeSourceType.FixedTitles)
                }));

            eventContext = new EventContext();

            interview = Create.AggregateRoot.Interview(questionnaireRepository: questionnaireRepository);
            var command = Create.Command.CreateInterview(
                questionnaireId: questionnaireId,
                questionnaireVersion: 1,
                responsibleSupervisorId: supervisorId,
                answersToFeaturedQuestions: answersToFeaturedQuestions,
                
                userId: userId);
            interview.CreateInterview(command);
        }

        [Test]
        public void should_not_raise_RosterInstancesRemoved_event() =>
            eventContext.ShouldNotContainEvent<RosterInstancesRemoved>();

        [Test]
        public void should_raise_RosterInstancesAdded_event_with_3_instances() =>
            eventContext.GetEvent<RosterInstancesAdded>().Instances.Count().Should().Be(3);

        [Test]
        public void should_set_roster_id_to_all_instances_in_RosterInstancesAdded_event() =>
            eventContext.GetEvent<RosterInstancesAdded>().Instances
                .Should().OnlyContain(instance => instance.GroupId == fixedRosterId);

        [Test]
        public void should_set_empty_outer_roster_vector_to_all_instances_in_RosterInstancesAdded_event() =>
            eventContext.GetEvent<RosterInstancesAdded>().Instances
                .Should().OnlyContain(instance => instance.OuterRosterVector.Length == 0);

        [Test]
        public void should_set__0__or__1__or_2__as_roster_instance_ids_in_RosterInstancesAdded_event() =>
            eventContext.GetEvent<RosterInstancesAdded>().Instances.Select(instance => instance.RosterInstanceId).ToArray()
                .Should().Equal(0, 1, 2);

        [Test]
        public void should_set_sort_0_1_2_as_index_to_all_instances_in_RosterInstancesAdded_event() =>
             eventContext.GetEvent<RosterInstancesAdded>().Instances
                .Select(x => x.SortIndex)
                .Should().Equal(0, 1, 2);

        [Test]
        public void should_raise_1_RosterRowsTitleChanged_events() =>
            eventContext.ShouldContainEvents<RosterInstancesTitleChanged>(count: 1);

        [Test]
        public void should_set_roster_id_to_all_RosterRowTitleChanged_events() =>
            eventContext.GetEvents<RosterInstancesTitleChanged>()
                .Should().OnlyContain(@event => @event.ChangedInstances.All(x => x.RosterInstance.GroupId == fixedRosterId));

        [Test]
        public void should_set_empty_outer_roster_vector_to_all_RosterRowTitleChanged_events() =>
            eventContext.GetEvents<RosterInstancesTitleChanged>()
                .Should().OnlyContain(@event => @event.ChangedInstances.All(x => x.RosterInstance.OuterRosterVector.Length == 0));

        [Test]
        public void should__0__or__1__or_2__as_roster_instance_ids_in_RosterRowsTitleChanged_event() =>
          eventContext.GetEvents<RosterInstancesTitleChanged>().SelectMany(@event => @event.ChangedInstances.Select(r => r.RosterInstance.RosterInstanceId)).ToArray()
              .Should().Equal(0, 1, 2);

        [Test]
        public void should_set_title__Title_1__in_RosterRowsTitleChanged_event_with_roster_instance_id_equal_to_0() =>
            eventContext.ShouldContainEvent<RosterInstancesTitleChanged>(
                @event => @event.ChangedInstances.Count(row => row.RosterInstance.RosterInstanceId == 0 && row.Title == "Title 1") == 1);

        [Test]
        public void should_set_title__Title_2__in_RosterRowsTitleChanged_event_with_roster_instance_id_equal_to_1() =>
             eventContext.ShouldContainEvent<RosterInstancesTitleChanged>(
                @event => @event.ChangedInstances.Count(row => row.RosterInstance.RosterInstanceId == 1 && row.Title == "Title 2") == 1);

        [Test]
        public void should_set_title__Title_3__in_RosterRowsTitleChanged_event_with_roster_instance_id_equal_to_2() =>
             eventContext.ShouldContainEvent<RosterInstancesTitleChanged>(
                @event => @event.ChangedInstances.Count(row => row.RosterInstance.RosterInstanceId == 2 && row.Title == "Title 3") == 1);

        [OneTimeTearDown]
        public void TearDown()
        {
            eventContext.Dispose();
            eventContext = null;
        }

        private static EventContext eventContext;
        private static Guid userId;
        private static Guid questionnaireId;
        private static DateTime answersTime;
        private static Guid supervisorId;
        private static Guid fixedRosterId;
        private static Interview interview;
    }
}
