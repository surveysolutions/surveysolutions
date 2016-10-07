using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;

namespace WB.Tests.Unit.SharedKernels.DataCollection
{
    [TestFixture]
    public class InterviewTreeEventPublisherTests
    {
        [Test]
        public void When_ApplyRosterEvents_and_2_rosters_removed_Then_RosterInstancesRemoved_event_with_2_roster_instances_should_be_applied()
        {
            //arrange
            var interviewId = Guid.Parse("11111111111111111111111111111111");
            var firstRosterIdentity = Create.Entity.Identity(Guid.Parse("22222222222222222222222222222222"), new[] {0m});
            var secondRosterIdentity = Create.Entity.Identity(Guid.Parse("33333333333333333333333333333333"), new[] {1m});
            var firstRoster = Create.Entity.InterviewTreeRoster(firstRosterIdentity);
            var secondRoster = Create.Entity.InterviewTreeRoster(secondRosterIdentity);
            var sectionIdentity = Create.Entity.Identity(Guid.Parse("44444444444444444444444444444444"), RosterVector.Empty);

            var mainSectionInSourceTree = Create.Entity.InterviewTreeSection(
                    sectionIdentity: sectionIdentity,
                    children: new IInterviewTreeNode[] {firstRoster, secondRoster});

            var mainSectionInChangedTree = Create.Entity.InterviewTreeSection(
                    sectionIdentity: sectionIdentity);

            var sourceTree = new InterviewTree(interviewId, new[] {mainSectionInSourceTree});
            var changedTree = new InterviewTree(interviewId, new[] {mainSectionInChangedTree});
            
            var listOfPublishedEvents = new List<IEvent>();
            var interview = InterviewTreeEventPublisher((evnt) => { listOfPublishedEvents.Add(evnt); });
            //act
            interview.ApplyRosterEvents(sourceTree, changedTree);
            //assert
            Assert.AreEqual(1, listOfPublishedEvents.Count);
            Assert.IsAssignableFrom<RosterInstancesRemoved>(listOfPublishedEvents[0]);

            var rosterInstancesRemovedEvent = ((RosterInstancesRemoved)listOfPublishedEvents[0]);
            Assert.AreEqual(2, rosterInstancesRemovedEvent.Instances.Length);
            Assert.AreEqual(firstRosterIdentity.Id, rosterInstancesRemovedEvent.Instances[0].GroupId);
            Assert.AreEqual(firstRosterIdentity.RosterVector.Shrink(), rosterInstancesRemovedEvent.Instances[0].OuterRosterVector);
            Assert.AreEqual(firstRosterIdentity.RosterVector.Last(), rosterInstancesRemovedEvent.Instances[0].RosterInstanceId);

            Assert.AreEqual(secondRosterIdentity.Id, rosterInstancesRemovedEvent.Instances[1].GroupId);
            Assert.AreEqual(secondRosterIdentity.RosterVector.Shrink(), rosterInstancesRemovedEvent.Instances[1].OuterRosterVector);
            Assert.AreEqual(secondRosterIdentity.RosterVector.Last(), rosterInstancesRemovedEvent.Instances[1].RosterInstanceId);
        }

        [Test]
        public void When_ApplyRosterEvents_and_roster_with_nested_roster_removed_Then_RosterInstancesRemoved_event_with_2_roster_instances_should_be_applied()
        {
            //arrange
            var interviewId = Guid.Parse("11111111111111111111111111111111");
            
            var nestedRosterIdentity = Create.Entity.Identity(Guid.Parse("22222222222222222222222222222222"), new[] { 0m, 0m });
            var nestedRoster = Create.Entity.InterviewTreeRoster(nestedRosterIdentity);

            var rosterIdentity = Create.Entity.Identity(Guid.Parse("33333333333333333333333333333333"), new[] { 0m });
            var roster = Create.Entity.InterviewTreeRoster(rosterIdentity, children: new IInterviewTreeNode[] {nestedRoster});

            var sectionIdentity = Create.Entity.Identity(Guid.Parse("44444444444444444444444444444444"), RosterVector.Empty);

            var mainSectionInSourceTree = Create.Entity.InterviewTreeSection(
                sectionIdentity: sectionIdentity,
                children: new IInterviewTreeNode[] {roster});

            var mainSectionInChangedTree = Create.Entity.InterviewTreeSection(
                    sectionIdentity: sectionIdentity);

            var sourceTree = new InterviewTree(interviewId, new[] { mainSectionInSourceTree });
            var changedTree = new InterviewTree(interviewId, new[] { mainSectionInChangedTree });

            var listOfPublishedEvents = new List<IEvent>();
            var interview = InterviewTreeEventPublisher((evnt) => { listOfPublishedEvents.Add(evnt); });
            //act
            interview.ApplyRosterEvents(sourceTree, changedTree);
            //assert
            Assert.AreEqual(1, listOfPublishedEvents.Count);
            Assert.IsAssignableFrom<RosterInstancesRemoved>(listOfPublishedEvents[0]);

            var rosterInstancesRemovedEvent = ((RosterInstancesRemoved)listOfPublishedEvents[0]);
            Assert.AreEqual(2, rosterInstancesRemovedEvent.Instances.Length);
            Assert.AreEqual(rosterIdentity.Id, rosterInstancesRemovedEvent.Instances[0].GroupId);
            Assert.AreEqual(rosterIdentity.RosterVector.Shrink(), rosterInstancesRemovedEvent.Instances[0].OuterRosterVector);
            Assert.AreEqual(rosterIdentity.RosterVector.Last(), rosterInstancesRemovedEvent.Instances[0].RosterInstanceId);

            Assert.AreEqual(nestedRosterIdentity.Id, rosterInstancesRemovedEvent.Instances[1].GroupId);
            Assert.AreEqual(nestedRosterIdentity.RosterVector.Shrink(), rosterInstancesRemovedEvent.Instances[1].OuterRosterVector);
            Assert.AreEqual(nestedRosterIdentity.RosterVector.Last(), rosterInstancesRemovedEvent.Instances[1].RosterInstanceId);
        }

        [Test]
        public void When_ApplyRosterEvents_and_2_rosters_added_Then_RosterInstancesAdded_event_with_2_roster_instances_should_be_applied()
        {
            //arrange
            var interviewId = Guid.Parse("11111111111111111111111111111111");
            var firstRosterIdentity = Create.Entity.Identity(Guid.Parse("22222222222222222222222222222222"), new[] { 0m });
            var secondRosterIdentity = Create.Entity.Identity(Guid.Parse("33333333333333333333333333333333"), new[] { 1m });
            var firstRoster = Create.Entity.InterviewTreeRoster(firstRosterIdentity);
            var secondRoster = Create.Entity.InterviewTreeRoster(secondRosterIdentity);
            var sectionIdentity = Create.Entity.Identity(Guid.Parse("44444444444444444444444444444444"), RosterVector.Empty);

            var mainSectionInSourceTree = Create.Entity.InterviewTreeSection(
                    sectionIdentity: sectionIdentity);

            var mainSectionInChangedTree = Create.Entity.InterviewTreeSection(
                    sectionIdentity: sectionIdentity,
                    children: new IInterviewTreeNode[] { firstRoster, secondRoster });

            var sourceTree = new InterviewTree(interviewId, new[] { mainSectionInSourceTree });
            var changedTree = new InterviewTree(interviewId, new[] { mainSectionInChangedTree });

            var listOfPublishedEvents = new List<IEvent>();
            var interview = InterviewTreeEventPublisher((evnt) => { listOfPublishedEvents.Add(evnt); });
            //act
            interview.ApplyRosterEvents(sourceTree, changedTree);
            //assert
            Assert.AreEqual(1, listOfPublishedEvents.Count);
            Assert.IsAssignableFrom<RosterInstancesAdded>(listOfPublishedEvents[0]);

            var rosterInstancesAddedEvent = ((RosterInstancesAdded)listOfPublishedEvents[0]);
            Assert.AreEqual(2, rosterInstancesAddedEvent.Instances.Length);
            Assert.AreEqual(firstRosterIdentity.Id, rosterInstancesAddedEvent.Instances[0].GroupId);
            Assert.AreEqual(firstRosterIdentity.RosterVector.Shrink(), rosterInstancesAddedEvent.Instances[0].OuterRosterVector);
            Assert.AreEqual(firstRosterIdentity.RosterVector.Last(), rosterInstancesAddedEvent.Instances[0].RosterInstanceId);

            Assert.AreEqual(secondRosterIdentity.Id, rosterInstancesAddedEvent.Instances[1].GroupId);
            Assert.AreEqual(secondRosterIdentity.RosterVector.Shrink(), rosterInstancesAddedEvent.Instances[1].OuterRosterVector);
            Assert.AreEqual(secondRosterIdentity.RosterVector.Last(), rosterInstancesAddedEvent.Instances[1].RosterInstanceId);
        }

        [Test]
        public void When_ApplyRosterEvents_and_roster_with_nested_roster_added_Then_RosterInstancesAdded_event_with_2_roster_instances_should_be_applied()
        {
            //arrange
            var interviewId = Guid.Parse("11111111111111111111111111111111");

            var nestedRosterIdentity = Create.Entity.Identity(Guid.Parse("22222222222222222222222222222222"), new[] { 0m, 0m });
            var nestedRoster = Create.Entity.InterviewTreeRoster(nestedRosterIdentity);

            var rosterIdentity = Create.Entity.Identity(Guid.Parse("33333333333333333333333333333333"), new[] { 0m });
            var roster = Create.Entity.InterviewTreeRoster(rosterIdentity, children: new IInterviewTreeNode[] { nestedRoster });

            var sectionIdentity = Create.Entity.Identity(Guid.Parse("44444444444444444444444444444444"), RosterVector.Empty);

            var mainSectionInSourceTree = Create.Entity.InterviewTreeSection(
                sectionIdentity: sectionIdentity);

            var mainSectionInChangedTree = Create.Entity.InterviewTreeSection(
                sectionIdentity: sectionIdentity,
                children: new IInterviewTreeNode[] {roster});

            var sourceTree = new InterviewTree(interviewId, new[] { mainSectionInSourceTree });
            var changedTree = new InterviewTree(interviewId, new[] { mainSectionInChangedTree });

            var listOfPublishedEvents = new List<IEvent>();
            var interview = InterviewTreeEventPublisher((evnt) => { listOfPublishedEvents.Add(evnt); });
            //act
            interview.ApplyRosterEvents(sourceTree, changedTree);
            //assert
            Assert.AreEqual(1, listOfPublishedEvents.Count);
            Assert.IsAssignableFrom<RosterInstancesAdded>(listOfPublishedEvents[0]);

            var rosterInstancesAddedEvent = ((RosterInstancesAdded)listOfPublishedEvents[0]);
            Assert.AreEqual(2, rosterInstancesAddedEvent.Instances.Length);
            Assert.AreEqual(rosterIdentity.Id, rosterInstancesAddedEvent.Instances[0].GroupId);
            Assert.AreEqual(rosterIdentity.RosterVector.Shrink(), rosterInstancesAddedEvent.Instances[0].OuterRosterVector);
            Assert.AreEqual(rosterIdentity.RosterVector.Last(), rosterInstancesAddedEvent.Instances[0].RosterInstanceId);

            Assert.AreEqual(nestedRosterIdentity.Id, rosterInstancesAddedEvent.Instances[1].GroupId);
            Assert.AreEqual(nestedRosterIdentity.RosterVector.Shrink(), rosterInstancesAddedEvent.Instances[1].OuterRosterVector);
            Assert.AreEqual(nestedRosterIdentity.RosterVector.Last(), rosterInstancesAddedEvent.Instances[1].RosterInstanceId);
        }

        [Test]
        public void When_ApplyRosterEvents_and_removed_roster_with_question_with_nested_roster_with_question_Then_RosterInstancesRemoved_event_with_2_roster_instances_and_AnswersRemoved_event_with_2_question_identities_should_be_applied()
        {
            //arrange
            var interviewId = Guid.Parse("11111111111111111111111111111111");

            var rosterVector = new[] { 0m };
            var nestedRosterVector = new[] { 0m, 0m };

            var rosterQuestionIdentity = Create.Entity.Identity(Guid.Parse("55555555555555555555555555555555"),
                rosterVector);
            var nestedRosterQuestionIdentity = Create.Entity.Identity(Guid.Parse("66666666666666666666666666666666"),
                nestedRosterVector);

            var rosterQuestion = Create.Entity.InterviewTreeQuestion(rosterQuestionIdentity);
            var nestedRosterQuestion = Create.Entity.InterviewTreeQuestion(nestedRosterQuestionIdentity);

            var nestedRosterIdentity = Create.Entity.Identity(Guid.Parse("22222222222222222222222222222222"), nestedRosterVector);
            var nestedRoster = Create.Entity.InterviewTreeRoster(nestedRosterIdentity, children: new IInterviewTreeNode[] {nestedRosterQuestion});

            
            var rosterIdentity = Create.Entity.Identity(Guid.Parse("33333333333333333333333333333333"), rosterVector);
            var roster = Create.Entity.InterviewTreeRoster(rosterIdentity, children: new IInterviewTreeNode[] { nestedRoster, rosterQuestion });

            var sectionIdentity = Create.Entity.Identity(Guid.Parse("44444444444444444444444444444444"), RosterVector.Empty);

            var mainSectionInSourceTree = Create.Entity.InterviewTreeSection(
                sectionIdentity: sectionIdentity,
                children: new IInterviewTreeNode[] { roster });

            var mainSectionInChangedTree = Create.Entity.InterviewTreeSection(
                    sectionIdentity: sectionIdentity);

            var sourceTree = new InterviewTree(interviewId, new[] { mainSectionInSourceTree });
            var changedTree = new InterviewTree(interviewId, new[] { mainSectionInChangedTree });

            var listOfPublishedEvents = new List<IEvent>();
            var interview = InterviewTreeEventPublisher((evnt) => { listOfPublishedEvents.Add(evnt); });
            //act
            interview.ApplyRosterEvents(sourceTree, changedTree);
            //assert
            Assert.AreEqual(2, listOfPublishedEvents.Count);
            Assert.IsAssignableFrom<RosterInstancesRemoved>(listOfPublishedEvents[0]);
            Assert.IsAssignableFrom<AnswersRemoved>(listOfPublishedEvents[1]);

            var rosterInstancesRemovedEvent = ((RosterInstancesRemoved)listOfPublishedEvents[0]);
            Assert.AreEqual(2, rosterInstancesRemovedEvent.Instances.Length);
            Assert.AreEqual(rosterIdentity.Id, rosterInstancesRemovedEvent.Instances[0].GroupId);
            Assert.AreEqual(rosterIdentity.RosterVector.Shrink(), rosterInstancesRemovedEvent.Instances[0].OuterRosterVector);
            Assert.AreEqual(rosterIdentity.RosterVector.Last(), rosterInstancesRemovedEvent.Instances[0].RosterInstanceId);

            Assert.AreEqual(nestedRosterIdentity.Id, rosterInstancesRemovedEvent.Instances[1].GroupId);
            Assert.AreEqual(nestedRosterIdentity.RosterVector.Shrink(), rosterInstancesRemovedEvent.Instances[1].OuterRosterVector);
            Assert.AreEqual(nestedRosterIdentity.RosterVector.Last(), rosterInstancesRemovedEvent.Instances[1].RosterInstanceId);

            var answersRemovedEvent = ((AnswersRemoved)listOfPublishedEvents[1]);
            Assert.AreEqual(2, answersRemovedEvent.Questions.Length);
            Assert.AreEqual(rosterQuestionIdentity.Id, answersRemovedEvent.Questions[0].Id);
            Assert.AreEqual(rosterQuestionIdentity.RosterVector, answersRemovedEvent.Questions[0].RosterVector);

            Assert.AreEqual(nestedRosterQuestionIdentity.Id, answersRemovedEvent.Questions[1].Id);
            Assert.AreEqual(nestedRosterQuestionIdentity.RosterVector, answersRemovedEvent.Questions[1].RosterVector);
        }

        [Test]
        public void When_ApplyRosterEvents_and_roster_with_nested_roster_changed_roster_titles_Then_2_RosterInstancesTitleChanged_events_should_be_applied()
        {
            //arrange
            var interviewId = Guid.Parse("11111111111111111111111111111111");

            var nestedRosterIdentity = Create.Entity.Identity(Guid.Parse("22222222222222222222222222222222"), new[] { 0m, 0m });
            var rosterIdentity = Create.Entity.Identity(Guid.Parse("33333333333333333333333333333333"), new[] { 0m });

            var nestedRoster = Create.Entity.InterviewTreeRoster(nestedRosterIdentity);
            var roster = Create.Entity.InterviewTreeRoster(rosterIdentity, children: new IInterviewTreeNode[] { nestedRoster });

            var changedNestedRoster = Create.Entity.InterviewTreeRoster(nestedRosterIdentity, rosterTitle: "changed nested roster title");
            var changedRoster = Create.Entity.InterviewTreeRoster(rosterIdentity, children: new IInterviewTreeNode[] { changedNestedRoster }, rosterTitle: "changed roster title");

            var sectionIdentity = Create.Entity.Identity(Guid.Parse("44444444444444444444444444444444"), RosterVector.Empty);

            var mainSectionInSourceTree = Create.Entity.InterviewTreeSection(
                sectionIdentity: sectionIdentity,
                children: new IInterviewTreeNode[] { roster });

            var mainSectionInChangedTree = Create.Entity.InterviewTreeSection(
                sectionIdentity: sectionIdentity,
                children: new IInterviewTreeNode[] { changedRoster });

            var sourceTree = new InterviewTree(interviewId, new[] { mainSectionInSourceTree });
            var changedTree = new InterviewTree(interviewId, new[] { mainSectionInChangedTree });

            var listOfPublishedEvents = new List<IEvent>();
            var interview = InterviewTreeEventPublisher((evnt) => { listOfPublishedEvents.Add(evnt); });
            //act
            interview.ApplyRosterEvents(sourceTree, changedTree);
            //assert
            Assert.AreEqual(1, listOfPublishedEvents.Count);
            Assert.IsAssignableFrom<RosterInstancesTitleChanged>(listOfPublishedEvents[0]);

            var rosterTitleChangedEvent = ((RosterInstancesTitleChanged)listOfPublishedEvents[0]);
            Assert.AreEqual(2, rosterTitleChangedEvent.ChangedInstances.Length);
            Assert.AreEqual(rosterIdentity.Id, rosterTitleChangedEvent.ChangedInstances[0].RosterInstance.GroupId);
            Assert.AreEqual(rosterIdentity.RosterVector.Shrink(), rosterTitleChangedEvent.ChangedInstances[0].RosterInstance.OuterRosterVector);
            Assert.AreEqual(rosterIdentity.RosterVector.Last(), rosterTitleChangedEvent.ChangedInstances[0].RosterInstance.RosterInstanceId);
            Assert.AreEqual(changedRoster.RosterTitle, rosterTitleChangedEvent.ChangedInstances[0].Title);

            Assert.AreEqual(nestedRosterIdentity.Id, rosterTitleChangedEvent.ChangedInstances[1].RosterInstance.GroupId);
            Assert.AreEqual(nestedRosterIdentity.RosterVector.Shrink(), rosterTitleChangedEvent.ChangedInstances[1].RosterInstance.OuterRosterVector);
            Assert.AreEqual(nestedRosterIdentity.RosterVector.Last(), rosterTitleChangedEvent.ChangedInstances[1].RosterInstance.RosterInstanceId);
            Assert.AreEqual(changedNestedRoster.RosterTitle, rosterTitleChangedEvent.ChangedInstances[1].Title);
        }

        private static Interview InterviewTreeEventPublisher(Action<IEvent> applyEvent)
        {
            var mockOfInterview = new Mock<Interview>();
            mockOfInterview.Setup(interview => interview.ApplyEvent(It.IsAny<IEvent>()))
                .Callback<IEvent>(applyEvent.Invoke);

            return mockOfInterview.Object;
        }
    }
}