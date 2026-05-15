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
        public async Task server_returns_Open_for_all_three_assignments_each_gets_correct_status()
        {
            // Arrange: server returns Open/Completed/Approved assignments at once
            var assignmentsRepo = Create.Storage.AssignmentDocumentsInmemoryStorage();

            var questionnaireId = Create.Entity.QuestionnaireIdentity(Id.gA);

            var openAssignment = new AssignmentApiView
            {
                Id = 1, Quantity = 5, QuestionnaireId = questionnaireId,
                Status = AssignmentStatus.Open, StatusComment = null
            };
            var completedAssignment = new AssignmentApiView
            {
                Id = 2, Quantity = 3, QuestionnaireId = questionnaireId,
                Status = AssignmentStatus.Completed, StatusComment = "Field done"
            };
            var approvedAssignment = new AssignmentApiView
            {
                Id = 3, Quantity = 2, QuestionnaireId = questionnaireId,
                Status = AssignmentStatus.Approved, StatusComment = "Approved"
            };

            var remoteDoc1 = Create.Entity.AssignmentApiDocument(1, 5, questionnaireId).Build();
            var remoteDoc2 = Create.Entity.AssignmentApiDocument(2, 3, questionnaireId).Build();
            var remoteDoc3 = Create.Entity.AssignmentApiDocument(3, 2, questionnaireId).Build();

            var questionnaireStorage = new Mock<WB.Core.SharedKernels.DataCollection.Repositories.IQuestionnaireStorage>();
            questionnaireStorage.Setup(s => s.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()))
                .Returns(Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocument()));

            var syncService = new Mock<ISynchronizationService>();
            syncService.Setup(s => s.GetAssignmentsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AssignmentApiView> { openAssignment, completedAssignment, approvedAssignment });
            syncService.Setup(s => s.GetAssignmentAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(remoteDoc1);
            syncService.Setup(s => s.GetAssignmentAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(remoteDoc2);
            syncService.Setup(s => s.GetAssignmentAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(remoteDoc3);
            syncService.Setup(s => s.LogAssignmentAsHandledAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var synchronizer = Create.Service.AssignmentsSynchronizer(
                synchronizationService: syncService.Object,
                assignmentsRepository: assignmentsRepo,
                questionnaireStorage: questionnaireStorage.Object
            );

            // Act
            await synchronizer.SynchronizeAssignmentsAsync(
                Mock.Of<IProgress<SyncProgressInfo>>(), new SynchronizationStatistics(), CancellationToken.None);

            // Assert
            var a1 = assignmentsRepo.GetById(1);
            a1.Status.Should().Be(AssignmentStatus.Open);
            a1.StatusComment.Should().BeNull();

            var a2 = assignmentsRepo.GetById(2);
            a2.Status.Should().Be(AssignmentStatus.Completed);
            a2.StatusComment.Should().Be("Field done");

            var a3 = assignmentsRepo.GetById(3);
            a3.Status.Should().Be(AssignmentStatus.Approved);
            a3.StatusComment.Should().Be("Approved");
        }

        [Test]
        public async Task server_reopens_Approved_assignment_local_becomes_Open()
        {
            // Arrange: local assignment is Approved (supervisor approved, offline), server now says Open (supervisor reopened)
            var local = Create.Entity
                .AssignmentDocument(10, 5, 0, Create.Entity.QuestionnaireIdentity(Id.gA).ToString())
                .Build();
            local.Status = AssignmentStatus.Approved;
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
                Status = AssignmentStatus.Approved, StatusComment = "Supervisor approved"
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
            updated.Status.Should().Be(AssignmentStatus.Approved);
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
                Status = AssignmentStatus.Approved, StatusComment = "Already approved"
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

            // Act
            await synchronizer.SynchronizeAssignmentsAsync(
                Mock.Of<IProgress<SyncProgressInfo>>(), new SynchronizationStatistics(), CancellationToken.None);

            // Assert: local status change was uploaded
            syncService.Verify(s => s.ChangeAssignmentStatusAsync(30, It.IsAny<AssignmentStatusChangeApiView>(), It.IsAny<CancellationToken>()), Times.Once);

            // After sync, server's Approved status overrides local Completed
            var updated = assignmentsRepo.GetById(30);
            updated.Status.Should().Be(AssignmentStatus.Approved);
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

            // Act
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
    }
}
