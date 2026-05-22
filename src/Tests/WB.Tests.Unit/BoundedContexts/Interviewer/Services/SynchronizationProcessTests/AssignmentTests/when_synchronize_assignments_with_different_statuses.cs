using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Assignment;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.SynchronizationProcessTests.AssignmentTests
{
    [TestFixture]
    public class when_synchronize_assignments_with_different_statuses
    {
        [Test]
        public async Task server_returns_only_Open_assignment_completed_is_removed_from_tablet()
        {
            // Arrange: interviewer has Open assignment locally; server (after interviewer completed it) returns
            // only Open assignments — the completed one is gone from the list → it should be deleted locally.
            var openLocal = Create.Entity
                .AssignmentDocument(1, 5, 0, Create.Entity.QuestionnaireIdentity(Id.gA).ToString())
                .Build();
            openLocal.Status = AssignmentStatus.Open;

            var completedLocal = Create.Entity
                .AssignmentDocument(2, 3, 0, Create.Entity.QuestionnaireIdentity(Id.gA).ToString())
                .Build();
            completedLocal.Status = AssignmentStatus.Completed;
            completedLocal.StatusComment = "Field done";

            var assignmentsRepo = Create.Storage.AssignmentDocumentsInmemoryStorage();
            assignmentsRepo.Store(new[] { openLocal, completedLocal });

            // Server only returns the still-Open assignment (Completed is kept on server, not sent back)
            var remoteOpen = new AssignmentApiView
            {
                Id = 1, Quantity = 5, QuestionnaireId = Create.Entity.QuestionnaireIdentity(Id.gA),
                Status = AssignmentStatus.Open
            };

            var syncService = new Mock<ISynchronizationService>();
            syncService.Setup(s => s.GetAssignmentsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AssignmentApiView> { remoteOpen });

            var synchronizer = Create.Service.AssignmentsSynchronizer(
                synchronizationService: syncService.Object,
                assignmentsRepository: assignmentsRepo
            );

            // Act
            await synchronizer.SynchronizeAssignmentsAsync(
                Mock.Of<IProgress<SyncProgressInfo>>(), new SynchronizationStatistics(), CancellationToken.None);

            // Assert: Open assignment stays, Completed is removed
            assignmentsRepo.GetById(1).Should().NotBeNull();
            assignmentsRepo.GetById(2).Should().BeNull("Completed assignment no longer returned by server should be removed");
        }

        [Test]
        public async Task server_reopens_Approved_assignment_local_becomes_Open()
        {
            // Arrange: local assignment is Approved (supervisor approved, offline), server now says Open (supervisor reopened)
            var local = Create.Entity
                .AssignmentDocument(10, 5, 0, Create.Entity.QuestionnaireIdentity(Id.gA).ToString())
                .Build();
            local.Status = AssignmentStatus.Closed;
            local.StatusComment = "Was approved";

            var assignmentsRepo = Create.Storage.AssignmentDocumentsInmemoryStorage();
            assignmentsRepo.Store(new[] { local });

            var remote = new AssignmentApiView
            {
                Id = 10, Quantity = 5, QuestionnaireId = Create.Entity.QuestionnaireIdentity(Id.gA),
                Status = AssignmentStatus.Open, StatusComment = "Reopened by HQ"
            };

            var syncService = new Mock<ISynchronizationService>();
            syncService.Setup(s => s.GetAssignmentsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AssignmentApiView> { remote });

            var synchronizer = Create.Service.AssignmentsSynchronizer(
                synchronizationService: syncService.Object,
                assignmentsRepository: assignmentsRepo
            );

            // Act
            await synchronizer.SynchronizeAssignmentsAsync(
                Mock.Of<IProgress<SyncProgressInfo>>(), new SynchronizationStatistics(), CancellationToken.None);

            // Assert: server's Open status wins
            var updated = assignmentsRepo.GetById(10);
            updated.Status.Should().Be(AssignmentStatus.Open);
            updated.StatusComment.Should().Be("Reopened by HQ");
            updated.StatusChangedAtUtc.Should().BeNull("pending flag cleared after server override");
        }

        [Test]
        public async Task server_approves_Completed_assignment_local_becomes_Approved()
        {
            // Arrange: interviewer completed assignment (Completed), supervisor approved it on server
            var local = Create.Entity
                .AssignmentDocument(20, 8, 0, Create.Entity.QuestionnaireIdentity(Id.gA).ToString())
                .Build();
            local.Status = AssignmentStatus.Completed;
            local.StatusComment = "Interviewer done";

            var assignmentsRepo = Create.Storage.AssignmentDocumentsInmemoryStorage();
            assignmentsRepo.Store(new[] { local });

            var remote = new AssignmentApiView
            {
                Id = 20, Quantity = 8, QuestionnaireId = Create.Entity.QuestionnaireIdentity(Id.gA),
                Status = AssignmentStatus.Closed, StatusComment = "Supervisor approved"
            };

            var syncService = new Mock<ISynchronizationService>();
            syncService.Setup(s => s.GetAssignmentsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AssignmentApiView> { remote });

            var synchronizer = Create.Service.AssignmentsSynchronizer(
                synchronizationService: syncService.Object,
                assignmentsRepository: assignmentsRepo
            );

            // Act
            await synchronizer.SynchronizeAssignmentsAsync(
                Mock.Of<IProgress<SyncProgressInfo>>(), new SynchronizationStatistics(), CancellationToken.None);

            // Assert
            var updated = assignmentsRepo.GetById(20);
            updated.Status.Should().Be(AssignmentStatus.Closed);
            updated.StatusComment.Should().Be("Supervisor approved");
        }

        [Test]
        public async Task pending_local_Completed_upload_and_server_already_Approved_are_both_handled()
        {
            // Arrange: local has Completed pending upload, but server already shows Approved
            // (supervisor approved on web before device synced — conflict)
            var local = Create.Entity
                .AssignmentDocument(30, 4, 0, Create.Entity.QuestionnaireIdentity(Id.gA).ToString())
                .Build();
            local.Status = AssignmentStatus.Completed;
            local.StatusComment = "Done by me";
            local.StatusChangedAtUtc = DateTime.UtcNow.AddMinutes(-5); // pending upload

            var assignmentsRepo = Create.Storage.AssignmentDocumentsInmemoryStorage();
            assignmentsRepo.Store(new[] { local });

            // Server already shows Approved — the upload will succeed (server can handle it gracefully),
            // and then the next GetAssignments returns Approved
            var remoteAfterUpload = new AssignmentApiView
            {
                Id = 30, Quantity = 4, QuestionnaireId = Create.Entity.QuestionnaireIdentity(Id.gA),
                Status = AssignmentStatus.Closed, StatusComment = "Already approved"
            };

            var syncService = new Mock<ISynchronizationService>();
            syncService.Setup(s => s.GetAssignmentsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AssignmentApiView> { remoteAfterUpload });
            syncService.Setup(s => s.ChangeAssignmentStatusAsync(30, It.IsAny<AssignmentStatusChangeApiView>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var synchronizer = Create.Service.AssignmentsSynchronizer(
                synchronizationService: syncService.Object,
                assignmentsRepository: assignmentsRepo
            );

            // Act: upload step runs first, then download/sync step
            await synchronizer.UploadLocalStatusChangesAsync(CancellationToken.None);
            await synchronizer.SynchronizeAssignmentsAsync(
                Mock.Of<IProgress<SyncProgressInfo>>(), new SynchronizationStatistics(), CancellationToken.None);

            // Assert: local status change was uploaded
            syncService.Verify(s => s.ChangeAssignmentStatusAsync(30, It.IsAny<AssignmentStatusChangeApiView>(), It.IsAny<CancellationToken>()), Times.Once);

            // After sync, server's Approved status overrides local Completed
            var updated = assignmentsRepo.GetById(30);
            updated.Status.Should().Be(AssignmentStatus.Closed);
            updated.StatusComment.Should().Be("Already approved");
            updated.StatusChangedAtUtc.Should().BeNull("pending flag is cleared");
        }

        [Test]
        public async Task multiple_pending_uploads_for_different_assignments_all_uploaded()
        {
            // Arrange: two assignments both have pending local status changes
            var local1 = Create.Entity
                .AssignmentDocument(41, 5, 0, Create.Entity.QuestionnaireIdentity(Id.gA).ToString())
                .Build();
            local1.Status = AssignmentStatus.Completed;
            local1.StatusComment = "Done A";
            local1.StatusChangedAtUtc = DateTime.UtcNow;

            var local2 = Create.Entity
                .AssignmentDocument(42, 3, 0, Create.Entity.QuestionnaireIdentity(Id.gA).ToString())
                .Build();
            local2.Status = AssignmentStatus.Open;
            local2.StatusComment = "Need more work";
            local2.StatusChangedAtUtc = DateTime.UtcNow;

            var assignmentsRepo = Create.Storage.AssignmentDocumentsInmemoryStorage();
            assignmentsRepo.Store(new[] { local1, local2 });

            var remote1 = new AssignmentApiView
                { Id = 41, Quantity = 5, QuestionnaireId = Create.Entity.QuestionnaireIdentity(Id.gA), Status = AssignmentStatus.Completed };
            var remote2 = new AssignmentApiView
                { Id = 42, Quantity = 3, QuestionnaireId = Create.Entity.QuestionnaireIdentity(Id.gA), Status = AssignmentStatus.Open };

            var syncService = new Mock<ISynchronizationService>();
            syncService.Setup(s => s.GetAssignmentsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AssignmentApiView> { remote1, remote2 });
            syncService.Setup(s => s.ChangeAssignmentStatusAsync(It.IsAny<int>(), It.IsAny<AssignmentStatusChangeApiView>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var synchronizer = Create.Service.AssignmentsSynchronizer(
                synchronizationService: syncService.Object,
                assignmentsRepository: assignmentsRepo
            );

            // Act: upload step runs first, then download/sync step
            await synchronizer.UploadLocalStatusChangesAsync(CancellationToken.None);
            await synchronizer.SynchronizeAssignmentsAsync(
                Mock.Of<IProgress<SyncProgressInfo>>(), new SynchronizationStatistics(), CancellationToken.None);

            // Assert: both uploads triggered
            syncService.Verify(s => s.ChangeAssignmentStatusAsync(41, It.IsAny<AssignmentStatusChangeApiView>(), It.IsAny<CancellationToken>()), Times.Once);
            syncService.Verify(s => s.ChangeAssignmentStatusAsync(42, It.IsAny<AssignmentStatusChangeApiView>(), It.IsAny<CancellationToken>()), Times.Once);

            // Pending flags cleared
            assignmentsRepo.GetById(41).StatusChangedAtUtc.Should().BeNull();
            assignmentsRepo.GetById(42).StatusChangedAtUtc.Should().BeNull();
        }

        [Test]
        public async Task assignment_without_pending_change_and_same_status_preserves_local_comment()
        {
            // Arrange: local and server both have Completed, same comment — no upload expected
            var local = Create.Entity
                .AssignmentDocument(50, 6, 0, Create.Entity.QuestionnaireIdentity(Id.gA).ToString())
                .Build();
            local.Status = AssignmentStatus.Completed;
            local.StatusComment = "Consistent comment";
            local.StatusChangedAtUtc = null; // no pending change

            var assignmentsRepo = Create.Storage.AssignmentDocumentsInmemoryStorage();
            assignmentsRepo.Store(new[] { local });

            var remote = new AssignmentApiView
            {
                Id = 50, Quantity = 6, QuestionnaireId = Create.Entity.QuestionnaireIdentity(Id.gA),
                Status = AssignmentStatus.Completed, StatusComment = "Consistent comment"
            };

            var syncService = new Mock<ISynchronizationService>();
            syncService.Setup(s => s.GetAssignmentsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AssignmentApiView> { remote });

            var synchronizer = Create.Service.AssignmentsSynchronizer(
                synchronizationService: syncService.Object,
                assignmentsRepository: assignmentsRepo
            );

            // Act
            await synchronizer.SynchronizeAssignmentsAsync(
                Mock.Of<IProgress<SyncProgressInfo>>(), new SynchronizationStatistics(), CancellationToken.None);

            // Assert: no upload for unchanged assignment
            syncService.Verify(s => s.ChangeAssignmentStatusAsync(It.IsAny<int>(), It.IsAny<AssignmentStatusChangeApiView>(), It.IsAny<CancellationToken>()), Times.Never);

            var updated = assignmentsRepo.GetById(50);
            updated.Status.Should().Be(AssignmentStatus.Completed);
            updated.StatusComment.Should().Be("Consistent comment");
        }

        [Test]
        public async Task upload_conflict_rejected_by_server_clears_pending_flag_and_does_not_fail_sync()
        {
            // Arrange: interviewer has Completed assignment with pending upload flag
            // but server rejects the upload (assignment already Approved by supervisor).
            var local = Create.Entity
                .AssignmentDocument(60, 4, 0, Create.Entity.QuestionnaireIdentity(Id.gA).ToString())
                .Build();
            local.Status = AssignmentStatus.Completed;
            local.StatusComment = "Done offline";
            local.StatusChangedAtUtc = DateTime.UtcNow.AddMinutes(-5); // pending upload

            var assignmentsRepo = Create.Storage.AssignmentDocumentsInmemoryStorage();
            assignmentsRepo.Store(new[] { local });

            // Server rejects upload (conflict: assignment is already in a state that doesn't allow Completed)
            var conflictException = new WB.Core.SharedKernels.Enumerator.Implementation.Services.SynchronizationException(
                WB.Core.SharedKernels.Enumerator.Implementation.Services.SynchronizationExceptionType.InvalidUrl,
                "Bad Request: Invalid status transition");

            // After the failed upload, server no longer returns this assignment (it's Approved → filtered out for interviewer)
            var syncService = new Mock<ISynchronizationService>();
            syncService.Setup(s => s.GetAssignmentsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AssignmentApiView>());
            syncService.Setup(s => s.ChangeAssignmentStatusAsync(60, It.IsAny<AssignmentStatusChangeApiView>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(conflictException);

            var synchronizer = Create.Service.AssignmentsSynchronizer(
                synchronizationService: syncService.Object,
                assignmentsRepository: assignmentsRepo
            );

            // Act: upload step should NOT throw even though upload was rejected
            Func<Task> uploadAct = () => synchronizer.UploadLocalStatusChangesAsync(CancellationToken.None);
            await uploadAct.Should().NotThrowAsync("conflict rejections must not abort the sync");

            // Then sync step removes the assignment (server no longer returns it)
            await synchronizer.SynchronizeAssignmentsAsync(
                Mock.Of<IProgress<SyncProgressInfo>>(), new SynchronizationStatistics(), CancellationToken.None);

            // Assert: upload was attempted, pending flag cleared, assignment removed (server didn't return it)
            syncService.Verify(s => s.ChangeAssignmentStatusAsync(60, It.IsAny<AssignmentStatusChangeApiView>(), It.IsAny<CancellationToken>()), Times.Once);
            assignmentsRepo.GetById(60).Should().BeNull("assignment was removed since server no longer returns it");
        }
    }
}
