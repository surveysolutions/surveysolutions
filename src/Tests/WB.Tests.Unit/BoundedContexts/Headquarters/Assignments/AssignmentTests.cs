using System;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Assignments
{
    [TestOf(typeof(Assignment))]
    public class AssignmentTests
    {
        [Test]
        public void should_calculate_interviews_needed_quantity_without_deleted_interviews()
        {
            var assignment = Create.Entity.Assignment(quantity: 2);

            assignment.InterviewSummaries.Add(Create.Entity.InterviewSummary());

            Assert.That(assignment.InterviewsNeeded, Is.EqualTo(1));
            Assert.That(assignment.IsCompleted, Is.False);
        }

        [Test]
        public void should_not_complete_assignment_when_it_is_unlimited()
        {
            var assignment = Create.Entity.Assignment(quantity: null);

            assignment.InterviewSummaries.Add(Create.Entity.InterviewSummary());

            Assert.That(assignment.IsCompleted, Is.False);
            Assert.That(assignment.InterviewsNeeded, Is.Null);
        }

        [Test]
        public void when_reassigned_should_recet_received_by_tablet()
        {
            var assignment = Create.Entity.Assignment(quantity: null);
            assignment.MarkAsReceivedByTablet();

            Assert.That(assignment.ReceivedByTabletAtUtc, Is.Not.Null);

            assignment.Reassign(Guid.NewGuid());

            Assert.That(assignment.ReceivedByTabletAtUtc, Is.Null);
        }
    }
}
