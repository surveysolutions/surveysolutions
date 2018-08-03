using System;
using System.Linq;
using FluentAssertions;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_hqapprove_interview : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            userId = Guid.Parse("AAAA0000AAAA00000000AAAA0000AAAA");
            supervisorId = Guid.Parse("BBAA0000AAAA00000000AAAA0000AAAA");
            questionnaireId = Guid.Parse("33333333333333333333333333333333");

            questionId = Guid.Parse("43333333333333333333333333333333");

            interview = Create.AggregateRoot.StatefulInterview(questionnaire: Create.Entity.QuestionnaireDocumentWithOneChapter());

            interview.AssignInterviewer(supervisorId, userId, DateTime.Now);
            interview.Apply(Create.Event.InterviewStatusChanged(status: InterviewStatus.Completed));
            interview.Approve(userId, string.Empty, DateTime.Now);

            eventContext = new EventContext();
            BecauseOf();
        }

        public void BecauseOf() =>
            interview.HqApprove(userId, string.Empty, DateTimeOffset.Now);

        [NUnit.Framework.Test] public void should_raise_two_events () =>
            eventContext.Events.Count().Should().Be(2);

        [NUnit.Framework.Test] public void should_raise_InterviewApprovedByHQ_event () =>
            eventContext.ShouldContainEvent<InterviewApprovedByHQ>(@event => @event.UserId == userId);

        [NUnit.Framework.Test] public void should_raise_InterviewStatusChanged_event () =>
            eventContext.ShouldContainEvent<InterviewStatusChanged>(@event => @event.Status == InterviewStatus.ApprovedByHeadquarters);

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            eventContext.Dispose();
            eventContext = null;
        }

        private static Guid userId;
        private static Guid supervisorId;

        private static Guid questionnaireId;
        private static Guid questionId;

        private static EventContext eventContext;
        private static Interview interview;
    }
}
