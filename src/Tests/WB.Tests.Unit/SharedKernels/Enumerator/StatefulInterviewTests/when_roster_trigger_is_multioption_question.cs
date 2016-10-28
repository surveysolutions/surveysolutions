using System;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Rosters
{
    [TestFixture]
    internal class when_roster_trigger_is_multioption_question : StatefulInterviewTestsContext
    {
        private Interview interview;
        private Guid triggerQuestionId;
        private Guid rosterId;
        private EventContext eventContext;
        private Guid firstChapterId;

        [OneTimeSetUp]
        public void Setup()
        {
            triggerQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rosterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.MultipleOptionsQuestion(
                    questionId: this.triggerQuestionId,
                    areAnswersOrdered: true,
                    answers: new []{ 5, 3, 2 }
                ),
                Create.Entity.Roster(
                    this.rosterId,
                    rosterSizeSourceType: RosterSizeSourceType.Question,
                    rosterSizeQuestionId: this.triggerQuestionId
                ));
            this.firstChapterId = questionnaire.Children[0].PublicKey;

            var questionnaires = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire);
            interview = Create.AggregateRoot.StatefulInterview(questionnaireRepository: questionnaires);

            eventContext = new EventContext();
        }

        [SetUp]
        public void BecauseOf()
        {
            this.interview.AnswerMultipleOptionsQuestion(Guid.NewGuid(), this.triggerQuestionId, RosterVector.Empty, DateTime.Now, new [] {2, 3});
        }

        [Test]
        public void It_should_put_first_roster_for_appropriate_option()
        {
            var rosterInstancesAdded = this.eventContext.GetEvent<RosterInstancesAdded>();
            Assert.That(rosterInstancesAdded, Is.Not.Null);
            var addedRosterInstance = rosterInstancesAdded.Instances[0];
            Assert.That(addedRosterInstance.RosterInstanceId, Is.EqualTo(2));
            Assert.That(addedRosterInstance.SortIndex, Is.EqualTo(0));
        }

        [Test]
        public void It_should_put_second_roster_for_appropriate_option()
        {
            var rosterInstancesAdded = this.eventContext.GetEvent<RosterInstancesAdded>();
            Assert.That(rosterInstancesAdded, Is.Not.Null);
            var addedRosterInstance = rosterInstancesAdded.Instances[1];
            Assert.That(addedRosterInstance.RosterInstanceId, Is.EqualTo(3));
            Assert.That(addedRosterInstance.SortIndex, Is.EqualTo(1));
        }

        [Test]
        public void It_should_return_roster_instances_sorted_by_sort_index()
        {
            var statefulInterview = (IStatefulInterview)this.interview;

            var rosterInstances = 
                statefulInterview.GetRosterInstances(Create.Entity.Identity(this.firstChapterId, RosterVector.Empty), this.rosterId);
            Assert.That(rosterInstances[0].RosterVector[0], Is.EqualTo(2));
            Assert.That(rosterInstances[1].RosterVector[0], Is.EqualTo(3));
        }

        [TearDown]
        public void TearDown()
        {
            this.eventContext.Dispose();
        }
    }
}