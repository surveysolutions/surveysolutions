using System;
using System.Linq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Interviews;
using WB.Core.GenericSubdomains.Portable;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Views.Interview
{
    [TestFixture]
    [TestOf(typeof(TeamInterviewsFactory))]
    public class TeamInterviewsFactoryTests
    {
        [Test]
        public void Should_find_interviews_by_assignment()
        {
            Guid interviewIdWithAssignment = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
            Guid interviewIdWithoutAssignment = Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");

            var summaryThatHasAssignmentId = Create.Entity.InterviewSummary(interviewIdWithAssignment, assignmentId: 5);
            var summaryWithoutAssignment = Create.Entity.InterviewSummary(interviewIdWithoutAssignment, assignmentId: null);

            var assignments = new TestInMemoryWriter<InterviewSummary>();
            assignments.Store(summaryThatHasAssignmentId, interviewIdWithAssignment.FormatGuid());
            assignments.Store(summaryWithoutAssignment, interviewIdWithoutAssignment.FormatGuid());

            var factory = Create.Service.TeamInterviewsFactory(assignments);

            // Act
            var foundEntries = factory.Load(new TeamInterviewsInputModel() { AssignmentId = 5 });

            // Assert
            Assert.That(foundEntries.TotalCount, Is.EqualTo(1));
            Assert.That(foundEntries.Items.Single().InterviewId, Is.EqualTo(interviewIdWithAssignment));
        }
    }
}