using System;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Synchronization
{
    public class when_synchronizing_interview_when_it_is_approved_by_sv_after_rejection_from_hq
    {
        [Test]
        public void should_be_able_to_synchronize()
        {
            var interviewId = Id.g1;
            var interviewerId = Id.gA;
            var supervisorId = Id.gB;
            var hqId = Id.gC;

            var interview = Create.AggregateRoot.StatefulInterview(interviewId, userId: interviewerId, supervisorId: supervisorId);
            interview.Complete(interviewId, "", DateTimeOffset.Now);
            interview.Approve(supervisorId, "", DateTimeOffset.Now);
            interview.HqReject(hqId, "", DateTimeOffset.Now);

            //interview.Approve(supervisorId, "", DateTime.UtcNow);

            // act
            using (var eventContext = new EventContext())
            {
                interview.SynchronizeInterviewEvents(
                    Create.Command.SynchronizeInterviewEventsCommand(interviewId, userId: supervisorId,
                        interviewStatus: InterviewStatus.ApprovedBySupervisor,
                        synchronizedEvents: new IEvent[]
                        {
                        Create.Event.InterviewApproved(supervisorId),
                        Create.Event.InterviewStatusChanged(InterviewStatus.ApprovedBySupervisor, previousStatus: InterviewStatus.RejectedByHeadquarters)
                        }));


                // Assert
                eventContext.ShouldContainEvent<InterviewApproved>();
                eventContext.ShouldContainEvent<InterviewStatusChanged>();
            }
        }
    }
}
