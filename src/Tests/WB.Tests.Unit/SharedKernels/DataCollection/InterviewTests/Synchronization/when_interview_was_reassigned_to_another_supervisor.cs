using System;
using NUnit.Framework;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Synchronization
{
    public class when_interview_was_reassigned_to_another_supervisor
    {
        [Test]
        public void should_throw_interview_exception()
        {
            var interviewId = Id.g1;
            var interviewerId = Id.gA;
            var supervisorId = Id.gB;
            var hqId = Id.gC;

            var interview = Create.AggregateRoot.StatefulInterview(interviewId, userId: interviewerId);
            interview.Complete(interviewId, "", DateTime.UtcNow);
            interview.Approve(supervisorId, "", DateTime.UtcNow);
            interview.HqReject(hqId, "");
            interview.AssignSupervisor(hqId, Id.gD);

            // act

            TestDelegate act = () => interview.SynchronizeInterviewEvents(
                Create.Command.SynchronizeInterviewEventsCommand(interviewId, userId: supervisorId,
                    interviewStatus: InterviewStatus.ApprovedBySupervisor,
                    synchronizedEvents: new IEvent[]
                    {
                        Create.Event.InterviewApproved(supervisorId),
                        Create.Event.InterviewStatusChanged(InterviewStatus.ApprovedBySupervisor,
                            previousStatus: InterviewStatus.RejectedByHeadquarters)
                    }));

            // Assert
            Assert.That(act, Throws.Exception.TypeOf<InterviewException>()
                .With.Property(nameof(InterviewException.ExceptionType)).EqualTo(InterviewDomainExceptionType.OtherUserIsResponsible));
        }
    }
}
