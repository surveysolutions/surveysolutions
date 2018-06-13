using System.Collections.Generic;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Assignments
{
    [TestOf(typeof(AssignmentsService))]
    public class AssignmentsServiceTests
    {
        [Test]
        public void when_assignment_has_no_protected_variables()
        {
            var interviewerId = Id.g1;

            var assignmentService = Create.Service.AssignmentService(
                Create.Entity.Assignment(quantity: null, responsibleId: interviewerId, protectedVariables: null) );

            // Act
            bool hasAssignmentWithProtectedVariables = assignmentService.HasAssignmentWithProtectedVariables(interviewerId);

            Assert.That(hasAssignmentWithProtectedVariables, Is.False);
        }

        [Test]
        public void when_assignment_has_protected_variables()
        {
            var interviewerId = Id.g1;

            var assignmentService = Create.Service.AssignmentService(
                Create.Entity.Assignment(quantity: null, responsibleId: interviewerId, protectedVariables: null),
                Create.Entity.Assignment(quantity: null, responsibleId: interviewerId, protectedVariables: new List<string>{ "var1", "var2" }) );

            // Act
            bool hasAssignmentWithProtectedVariables = assignmentService.HasAssignmentWithProtectedVariables(interviewerId);

            Assert.That(hasAssignmentWithProtectedVariables, Is.True);
        }
    }
}
