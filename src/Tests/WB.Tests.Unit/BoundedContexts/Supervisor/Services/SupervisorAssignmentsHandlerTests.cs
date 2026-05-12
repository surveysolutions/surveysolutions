using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Supervisor.Services.Implementation.OfflineSyncHandlers;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Assignment;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Supervisor.Services
{
    [TestOf(typeof(SupervisorAssignmentsHandler))]
    public class SupervisorAssignmentsHandlerTests
    {
        [Test]
        public async Task GetAssignments_should_set_is_audio_enabled_flag()
        {
            var interviewerId = Id.g1;

            var assignment = Create.Entity.AssignmentDocument(1, 
                                                             quantity: 5,
                                                             interviewsCount: 2, 
                                                             questionnaireIdentity: Create.Entity.QuestionnaireIdentity().ToString())
                                          .WithResponsible(interviewerId).Build();
            assignment.IsAudioRecordingEnabled = true;

            var assignments = Create.Storage.AssignmentDocumentsInmemoryStorage();
            assignments.Store(assignment);
            var handler = Create.Service.SupervisorAssignmentsHandler(assignments);

            // Act
            var assignmentFromResponse = await handler.GetAssignments(new GetAssignmentsRequest
            {
                UserId = interviewerId
            });

            // Assert
            Assert.That(assignmentFromResponse.Assignments[0].IsAudioRecordingEnabled, Is.True);
        }

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

        [Test]
        public async Task GetAssignments_should_include_status_and_comment()
        {
            var interviewerId = Id.g1;

            var assignment = Create.Entity.AssignmentDocument(1, quantity: 5, interviewsCount: 0,
                questionnaireIdentity: Create.Entity.QuestionnaireIdentity().ToString())
                .WithResponsible(interviewerId).Build();
            assignment.Status = AssignmentStatus.Finished;
            assignment.StatusComment = "Done for the day";

            var assignments = Create.Storage.AssignmentDocumentsInmemoryStorage();
            assignments.Store(assignment);
            var handler = Create.Service.SupervisorAssignmentsHandler(assignments);

            // Act
            var response = await handler.GetAssignments(new GetAssignmentsRequest { UserId = interviewerId });

            // Assert
            response.Assignments[0].Status.Should().Be(AssignmentStatus.Finished);
            response.Assignments[0].StatusComment.Should().Be("Done for the day");
        }

        [Test]
        public async Task ChangeAssignmentStatus_when_active_applies_interviewer_finish()
        {
            var assignment = Create.Entity.AssignmentDocument(1, quantity: 5, interviewsCount: 0,
                questionnaireIdentity: Create.Entity.QuestionnaireIdentity().ToString())
                .Build();
            assignment.Status = AssignmentStatus.Active;

            var assignments = Create.Storage.AssignmentDocumentsInmemoryStorage();
            assignments.Store(assignment);
            var handler = Create.Service.SupervisorAssignmentsHandler(assignments);

            // Act
            await handler.ChangeAssignmentStatus(new ChangeAssignmentStatusRequest
            {
                Id = 1,
                StatusChange = new AssignmentStatusChangeApiView
                {
                    Status = AssignmentStatus.Finished,
                    Comment = "No more units"
                }
            });

            // Assert
            var updated = assignments.GetById(1);
            updated.Status.Should().Be(AssignmentStatus.Finished);
            updated.StatusComment.Should().Be("No more units");
            updated.StatusChangedAtUtc.Should().NotBeNull();
        }

        [Test]
        public async Task ChangeAssignmentStatus_when_supervisor_already_completed_ignores_interviewer_finish()
        {
            var assignment = Create.Entity.AssignmentDocument(1, quantity: 5, interviewsCount: 0,
                questionnaireIdentity: Create.Entity.QuestionnaireIdentity().ToString())
                .Build();
            assignment.Status = AssignmentStatus.Completed; // supervisor already completed

            var assignments = Create.Storage.AssignmentDocumentsInmemoryStorage();
            assignments.Store(assignment);
            var handler = Create.Service.SupervisorAssignmentsHandler(assignments);

            // Act: interviewer tries to Finish, but supervisor has already Completed
            await handler.ChangeAssignmentStatus(new ChangeAssignmentStatusRequest
            {
                Id = 1,
                StatusChange = new AssignmentStatusChangeApiView
                {
                    Status = AssignmentStatus.Finished,
                    Comment = "I think I'm done"
                }
            });

            // Assert: supervisor's Completed status is preserved
            var updated = assignments.GetById(1);
            updated.Status.Should().Be(AssignmentStatus.Completed, "supervisor overrides interviewer");
        }
    }
}
