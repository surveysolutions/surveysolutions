using System;
using System.Linq;
using FluentAssertions;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    [TestFixture]
    [TestOf(typeof(Interview))]
    internal class when_hqreject_interview : InterviewTestsContext
    {
        [SetUp]
        public void SetUp()
        { 
            userId = Guid.Parse("AAAA0000AAAA00000000AAAA0000AAAA");
            supervisorId = Guid.Parse("BBAA0000AAAA00000000AAAA0000AAAA");
            questionnaireId = Guid.Parse("33333333333333333333333333333333");

            interview = Create.AggregateRoot.StatefulInterview(questionnaire: Create.Entity.QuestionnaireDocumentWithOneChapter());

            interview.AssignInterviewer(supervisorId, userId, DateTime.Now);
            interview.Apply(Create.Event.InterviewStatusChanged(status: InterviewStatus.Completed));
            interview.Approve(userId, string.Empty, DateTime.Now);

            eventContext = new EventContext();

            interview.HqReject(userId, string.Empty, DateTimeOffset.Now);
        }


        [Test]
        public void should_raise_two_events()
        {
            eventContext.Events.Count().Should().Be(2);
        }

        [Test]
        public void should_raise_InterviewApprovedByHQ_event()
        {
            eventContext.ShouldContainEvent<InterviewRejectedByHQ>(@event => @event.UserId == userId);
        }

        [Test]
        public void should_raise_InterviewStatusChanged_event()
        {
            eventContext.ShouldContainEvent<InterviewStatusChanged>(@event => @event.Status == InterviewStatus.RejectedByHeadquarters);
        }

        [TearDown]
        public void Stuff()
        {
            eventContext.Dispose();
            eventContext = null;
        }

        private static Guid userId;
        private static Guid supervisorId;

        private static Guid questionnaireId;
        private static EventContext eventContext;
        private static Interview interview;
    }
}
