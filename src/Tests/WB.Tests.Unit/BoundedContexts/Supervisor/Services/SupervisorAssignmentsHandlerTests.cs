using System.Threading.Tasks;
using NUnit.Framework;
using WB.Core.BoundedContexts.Supervisor.Services.Implementation.OfflineSyncHandlers;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Supervisor.Services
{
    [TestOf(typeof(SupervisorAssignmentsHandler))]
    public class SupervisorAssignmentsHandlerTests
    {
        [Test]
        public async Task GetAssignments_should_reduce_assignment_size_when_pushing_interviews_to_IN()
        {
            var interviewerId = Id.g1;

            var assignment = Create.Entity.AssignmentDocument(1, quantity: 5, interviewsCount: 2, questionnaireIdentity: Create.Entity.QuestionnaireIdentity().ToString())
                .WithResponsible(interviewerId).Build();
            var assignments = Create.Storage.AssignmentDocumentsInmemoryStorage();
            assignments.Store(assignment);
            var handler = Create.Service.SupervisorAssignmentsHandler(assignments);

            // Act
            var assignmentFromResponse = await handler.GetAssignments(new GetAssignmentsRequest
            {
                UserId = interviewerId
            });

            // Assert
            var quantity = assignmentFromResponse.Assignments[0].Quantity;
            Assert.That(quantity, Is.Not.Null);
            Assert.That(quantity, Is.EqualTo(assignment.Quantity - assignment.CreatedInterviewsCount));
        }

        [Test]
        public async Task GetAssignments_should_not_modify_unlimited_assignment()
        {
            var interviewerId = Id.g1;

            var assignment = Create.Entity.AssignmentDocument(1, quantity: null, interviewsCount: 2, questionnaireIdentity: Create.Entity.QuestionnaireIdentity().ToString())
                .WithResponsible(interviewerId).Build();
            var assignments = Create.Storage.AssignmentDocumentsInmemoryStorage();
            assignments.Store(assignment);
            var handler = Create.Service.SupervisorAssignmentsHandler(assignments);

            // Act
            var assignmentFromResponse = await handler.GetAssignments(new GetAssignmentsRequest
            {
                UserId = interviewerId
            });

            // Assert
            var quantity = assignmentFromResponse.Assignments[0].Quantity;
            Assert.That(quantity, Is.Null);
        }
    }
}
