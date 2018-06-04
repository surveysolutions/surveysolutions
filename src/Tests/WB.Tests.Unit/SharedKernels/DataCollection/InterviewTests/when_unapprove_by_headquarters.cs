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
    internal class when_unapprove_by_headquarters : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            userId = Guid.Parse("AAAA0000AAAA00000000AAAA0000AAAA");
            supervisorId = Guid.Parse("BBAA0000AAAA00000000AAAA0000AAAA");
            questionnaireId = Guid.Parse("33333333333333333333333333333333");

            questionId = Guid.Parse("43333333333333333333333333333333");

            interview = Create.AggregateRoot.StatefulInterview(questionnaire: Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId));

            interview.AssignInterviewer(supervisorId, userId, DateTimeOffset.Now);
            interview.Apply(Create.Event.InterviewStatusChanged(status: InterviewStatus.Completed));
            interview.Approve(userId, string.Empty, DateTimeOffset.Now);
            interview.HqApprove(userId, string.Empty, DateTimeOffset.Now);

            eventContext = new EventContext();
            BecauseOf();
        }

        public void BecauseOf() =>
            interview.UnapproveByHeadquarters(userId, string.Empty, DateTimeOffset.Now);

        [NUnit.Framework.Test] public void should_raise_two_events () =>
            eventContext.Events.Count().Should().Be(2);

        [NUnit.Framework.Test] public void should_raise_InterviewUnapprovedByHQ_event () =>
            eventContext.ShouldContainEvent<UnapprovedByHeadquarters>(@event => @event.UserId == userId);

        [NUnit.Framework.Test] public void should_raise_InterviewUnapprovedByHQ_with_comment () =>
            eventContext.GetEvent<UnapprovedByHeadquarters>().Comment.Should().Contain("[Approved by Headquarters was revoked]");

        [NUnit.Framework.Test] public void should_raise_InterviewStatusChanged_event () =>
            eventContext.ShouldContainEvent<InterviewStatusChanged>(@event => @event.Status == InterviewStatus.ApprovedBySupervisor);

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
