using System;
using System.Linq;
using FluentAssertions;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_interview_is_restarted : InterviewTestsContext
    {
        [Test]
        public void Interview__NOT__received_by_interviewer_should_change_status_to_interviewer_assigned()
        {
            var userId = Id.gA;

            var interview = Create.AggregateRoot.StatefulInterview(Id.g1, userId: userId, questionnaire: Create.Entity.QuestionnaireDocumentWithOneChapter());
            interview.Complete(userId, null, DateTime.Now);

            using (var eventContext = new EventContext())
            {
                interview.Restart(userId, null, DateTime.Now);

                eventContext.ShouldContainEvent<InterviewStatusChanged>(x => x.Status == InterviewStatus.InterviewerAssigned);
                eventContext.ShouldContainEvent<InterviewStatusChanged>(x => x.Status == InterviewStatus.Restarted);

                var statusChangedEvents = eventContext.GetEvents<InterviewStatusChanged>();

                var restartedStatusChangeEvent = statusChangedEvents.Single(x => x.Status == InterviewStatus.Restarted);
                var interviewerAssignedStatusChangeEvent = statusChangedEvents.Single(x => x.Status == InterviewStatus.InterviewerAssigned);
                statusChangedEvents.Should().HaveElementSucceeding(restartedStatusChangeEvent, interviewerAssignedStatusChangeEvent, because: "InterviewerAssigned status should go after Restarted");
            }
        }
    }
}