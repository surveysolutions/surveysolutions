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
            assignment.Status = AssignmentStatus.Completed;
            assignment.StatusComment = "Done for the day";

            var assignments = Create.Storage.AssignmentDocumentsInmemoryStorage();
            assignments.Store(assignment);
            var handler = Create.Service.SupervisorAssignmentsHandler(assignments);

            // Act
            var response = await handler.GetAssignments(new GetAssignmentsRequest { UserId = interviewerId });

            // Assert
            response.Assignments[0].Status.Should().Be(AssignmentStatus.Completed);
            response.Assignments[0].StatusComment.Should().Be("Done for the day");
        }

        [Test]
        public async Task ChangeAssignmentStatus_when_active_applies_interviewer_finish()
        {
            var assignment = Create.Entity.AssignmentDocument(1, quantity: 5, interviewsCount: 0,
                questionnaireIdentity: Create.Entity.QuestionnaireIdentity().ToString())
                .Build();
            assignment.Status = AssignmentStatus.Open;

            var assignments = Create.Storage.AssignmentDocumentsInmemoryStorage();
            assignments.Store(assignment);
            var handler = Create.Service.SupervisorAssignmentsHandler(assignments);

            // Act
            await handler.ChangeAssignmentStatus(new ChangeAssignmentStatusRequest
            {
                Id = 1,
                StatusChange = new AssignmentStatusChangeApiView
                {
                    Status = AssignmentStatus.Completed,
                    Comment = "No more units"
                }
            });

            // Assert
            var updated = assignments.GetById(1);
            updated.Status.Should().Be(AssignmentStatus.Completed);
            updated.StatusComment.Should().Be("No more units");
            updated.StatusChangedAtUtc.Should().NotBeNull();
        }

        [Test]
        public async Task ChangeAssignmentStatus_when_supervisor_already_completed_ignores_interviewer_finish()
        {
            var assignment = Create.Entity.AssignmentDocument(1, quantity: 5, interviewsCount: 0,
                questionnaireIdentity: Create.Entity.QuestionnaireIdentity().ToString())
                .Build();
            assignment.Status = AssignmentStatus.Approved; // supervisor already completed

            var assignments = Create.Storage.AssignmentDocumentsInmemoryStorage();
            assignments.Store(assignment);
            var handler = Create.Service.SupervisorAssignmentsHandler(assignments);

            // Act: interviewer tries to Finish, but supervisor has already Completed
            await handler.ChangeAssignmentStatus(new ChangeAssignmentStatusRequest
            {
                Id = 1,
                StatusChange = new AssignmentStatusChangeApiView
                {
                    Status = AssignmentStatus.Completed,
                    Comment = "I think I'm done"
                }
            });

            // Assert: supervisor's Completed status is preserved
            var updated = assignments.GetById(1);
            updated.Status.Should().Be(AssignmentStatus.Approved, "supervisor overrides interviewer");
        }

        [Test]
        public async Task ChangeAssignmentStatus_when_assignment_not_found_returns_ok_without_crash()
        {
            var handler = Create.Service.SupervisorAssignmentsHandler();

            // Act: request for non-existent assignment - should not throw
            Func<Task> act = () => handler.ChangeAssignmentStatus(new ChangeAssignmentStatusRequest
            {
                Id = 999,
                StatusChange = new AssignmentStatusChangeApiView
                {
                    Status = AssignmentStatus.Completed
                }
            });

            // Assert: completes without throwing
            await act.Should().NotThrowAsync();
        }

        [Test]
        public async Task ChangeAssignmentStatus_when_already_completed_ignores_interviewer_reopen()
        {
            var assignment = Create.Entity.AssignmentDocument(1, quantity: 5, interviewsCount: 0,
                questionnaireIdentity: Create.Entity.QuestionnaireIdentity().ToString())
                .Build();
            assignment.Status = AssignmentStatus.Completed;
            assignment.StatusComment = "Field finished";

            var assignments = Create.Storage.AssignmentDocumentsInmemoryStorage();
            assignments.Store(assignment);
            var handler = Create.Service.SupervisorAssignmentsHandler(assignments);

            // Act: interviewer tries to Reopen, but assignment is already Completed on supervisor side
            await handler.ChangeAssignmentStatus(new ChangeAssignmentStatusRequest
            {
                Id = 1,
                StatusChange = new AssignmentStatusChangeApiView
                {
                    Status = AssignmentStatus.Open,
                    Comment = "Oops, need to redo"
                }
            });

            // Assert: Completed status preserved; supervisor change takes precedence
            var updated = assignments.GetById(1);
            updated.Status.Should().Be(AssignmentStatus.Completed, "supervisor's Completed status is not overridden by interviewer");
            updated.StatusComment.Should().Be("Field finished", "comment is not changed by ignored request");
        }

        [Test]
        public async Task ChangeAssignmentStatus_when_status_change_is_null_defaults_to_open_and_applies_when_open()
        {
            var assignment = Create.Entity.AssignmentDocument(1, quantity: 5, interviewsCount: 0,
                questionnaireIdentity: Create.Entity.QuestionnaireIdentity().ToString())
                .Build();
            assignment.Status = AssignmentStatus.Open;

            var assignments = Create.Storage.AssignmentDocumentsInmemoryStorage();
            assignments.Store(assignment);
            var handler = Create.Service.SupervisorAssignmentsHandler(assignments);

            // Act: send request with null StatusChange
            await handler.ChangeAssignmentStatus(new ChangeAssignmentStatusRequest
            {
                Id = 1,
                StatusChange = null
            });

            // Assert: status defaults to Open, assignment remains Open
            var updated = assignments.GetById(1);
            updated.Status.Should().Be(AssignmentStatus.Open);
        }

        [Test]
        public async Task GetAssignments_should_return_open_completed_and_approved_assignments()
        {
            var interviewerId = Id.g1;

            var openDoc = Create.Entity.AssignmentDocument(1, quantity: 1, interviewsCount: 0,
                questionnaireIdentity: Create.Entity.QuestionnaireIdentity().ToString())
                .WithResponsible(interviewerId).Build();
            openDoc.Status = AssignmentStatus.Open;

            var completedDoc = Create.Entity.AssignmentDocument(2, quantity: 1, interviewsCount: 0,
                questionnaireIdentity: Create.Entity.QuestionnaireIdentity().ToString())
                .WithResponsible(interviewerId).Build();
            completedDoc.Status = AssignmentStatus.Completed;
            completedDoc.StatusComment = "Done";

            var approvedDoc = Create.Entity.AssignmentDocument(3, quantity: 1, interviewsCount: 0,
                questionnaireIdentity: Create.Entity.QuestionnaireIdentity().ToString())
                .WithResponsible(interviewerId).Build();
            approvedDoc.Status = AssignmentStatus.Approved;
            approvedDoc.StatusComment = "All good";

            var assignments = Create.Storage.AssignmentDocumentsInmemoryStorage();
            assignments.Store(openDoc);
            assignments.Store(completedDoc);
            assignments.Store(approvedDoc);
            var handler = Create.Service.SupervisorAssignmentsHandler(assignments);

            // Act
            var response = await handler.GetAssignments(new GetAssignmentsRequest { UserId = interviewerId });

            // Assert: all three assignments returned with correct statuses
            response.Assignments.Should().HaveCount(3);
            response.Assignments.Should().Contain(a => a.Status == AssignmentStatus.Open && a.Id == 1);
            response.Assignments.Should().Contain(a => a.Status == AssignmentStatus.Completed && a.Id == 2 && a.StatusComment == "Done");
            response.Assignments.Should().Contain(a => a.Status == AssignmentStatus.Approved && a.Id == 3 && a.StatusComment == "All good");
        }
    }
}
