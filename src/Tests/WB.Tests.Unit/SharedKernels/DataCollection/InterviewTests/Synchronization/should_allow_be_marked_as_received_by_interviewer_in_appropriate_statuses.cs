using System;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Synchronization
{
    public class should_allow_be_marked_as_received_by_interviewer : InterviewTestsContext
    {
        [TestCase(InterviewStatus.InterviewerAssigned)]
        [TestCase(InterviewStatus.SupervisorAssigned)]
        [TestCase(InterviewStatus.RejectedBySupervisor)]
        [TestCase(InterviewStatus.RejectedByHeadquarters)]
        public void should_allow_command_in_status(InterviewStatus targetStatus)
        {
            var interview = Create.AggregateRoot.StatefulInterview(Id.gA);
            interview.Apply(Create.Event.InterviewStatusChanged(targetStatus));

            // Act
            TestDelegate act = () => interview.MarkInterviewAsReceivedByInterviewer(Id.g1, null, DateTimeOffset.UtcNow);

            // Assert
            Assert.That(act, Throws.Nothing);
        }

        [TestCase(InterviewStatus.ApprovedByHeadquarters)]
        [TestCase(InterviewStatus.ApprovedBySupervisor)]
        [TestCase(InterviewStatus.Completed)]
        public void should_not_allow_command_in_statuses(InterviewStatus targetStatus)
        {
            var interview = Create.AggregateRoot.StatefulInterview(Id.gA);
            interview.Apply(Create.Event.InterviewStatusChanged(targetStatus));

            // Act
            TestDelegate act = () => interview.MarkInterviewAsReceivedByInterviewer(Id.g1, null, DateTimeOffset.UtcNow);

            // Assert
            Assert.That(act, Throws.Exception.InstanceOf<InterviewException>());
        }
    }
}
