using System;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Synchronization
{
    public class should_allow_be_marked_as_received_by_supervisor_in_appropriate_statuses : InterviewTestsContext
    {
        [TestCase(InterviewStatus.InterviewerAssigned)]
        [TestCase(InterviewStatus.SupervisorAssigned)]
        [TestCase(InterviewStatus.RejectedBySupervisor)]
        [TestCase(InterviewStatus.RejectedByHeadquarters)]
        public void should_allow_MarkInterviewAsReceivedBySupervisor_command_in_status(InterviewStatus targetStatus)
        {
            var interview = Create.AggregateRoot.StatefulInterview(Id.gA);
            interview.Apply(Create.Event.InterviewStatusChanged(targetStatus));

            // Act
            TestDelegate act = () => interview.MarkInterviewAsReceivedBySupervisor(Id.g1, DateTimeOffset.UtcNow);

            // Assert
            Assert.That(act, Throws.Nothing);
        }

        [TestCase(InterviewStatus.Deleted)]
        [TestCase(InterviewStatus.Restored)]
        [TestCase(InterviewStatus.Created)]
        [TestCase(InterviewStatus.Restarted)]
        [TestCase(InterviewStatus.Completed)]
        [TestCase(InterviewStatus.ApprovedBySupervisor)]
        [TestCase(InterviewStatus.ApprovedByHeadquarters)]
        public void should_not_allow_MarkInterviewAsReceivedBySupervisor_command_in_statuses(InterviewStatus targetStatus)
        {
            var interview = Create.AggregateRoot.StatefulInterview(Id.gA);
            interview.Apply(Create.Event.InterviewStatusChanged(targetStatus));

            // Act
            TestDelegate act = () => interview.MarkInterviewAsReceivedByInterviewer(Id.g1, DateTimeOffset.UtcNow);

            // Assert
            Assert.That(act, Throws.Exception.InstanceOf<InterviewException>());
        }
    }
}
