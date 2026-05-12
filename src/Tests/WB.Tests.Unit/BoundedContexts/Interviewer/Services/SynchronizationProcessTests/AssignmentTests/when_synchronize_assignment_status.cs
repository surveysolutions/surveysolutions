using System;
using System.Collections.Generic;
using System.Linq;
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
    public class when_synchronize_assignment_status
    {
        [Test]
        public async Task server_status_overrides_local_status()
        {
            // Arrange: local has Finished (no pending upload), server returns Active
            var localAssignment = Create.Entity
                .AssignmentDocument(1, 10, 0, Create.Entity.QuestionnaireIdentity(Id.gA).ToString())
                .Build();
            localAssignment.Status = AssignmentStatus.Finished;
            localAssignment.StatusComment = "server comment";
            // No pending upload (StatusChangedAtUtc is null)

            var assignmentsRepo = Create.Storage.AssignmentDocumentsInmemoryStorage();
            assignmentsRepo.Store(new[] { localAssignment });

            var remoteView = new AssignmentApiView
            {
                Id = 1,
                Quantity = 10,
                QuestionnaireId = Create.Entity.QuestionnaireIdentity(Id.gA),
                Status = AssignmentStatus.Active, // server has Active
                StatusComment = null
            };

            var syncService = new Mock<ISynchronizationService>();
            syncService.Setup(s => s.GetAssignmentsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AssignmentApiView> { remoteView });

            var synchronizer = Create.Service.AssignmentsSynchronizer(
                synchronizationService: syncService.Object,
                assignmentsRepository: assignmentsRepo
            );

            // Act
            await synchronizer.SynchronizeAssignmentsAsync(Mock.Of<IProgress<SyncProgressInfo>>(), new SynchronizationStatistics(), CancellationToken.None);

            // Assert: local status should be overridden to Active by server
            var updated = assignmentsRepo.GetById(1);
            updated.Status.Should().Be(AssignmentStatus.Active);
            updated.StatusComment.Should().BeNull("server sent null status comment");
            updated.StatusChangedAtUtc.Should().BeNull("pending flag is cleared after sync");
            // No upload should have been made since there was no pending change
            syncService.Verify(s => s.ChangeAssignmentStatusAsync(It.IsAny<int>(), It.IsAny<AssignmentStatusChangeApiView>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task server_completed_status_overrides_local_finished()
        {
            // Arrange: local has Finished (no pending upload), server has Completed (supervisor completed it)
            var localAssignment = Create.Entity
                .AssignmentDocument(1, 10, 0, Create.Entity.QuestionnaireIdentity(Id.gA).ToString())
                .Build();
            localAssignment.Status = AssignmentStatus.Finished;

            var assignmentsRepo = Create.Storage.AssignmentDocumentsInmemoryStorage();
            assignmentsRepo.Store(new[] { localAssignment });

            var remoteView = new AssignmentApiView
            {
                Id = 1,
                Quantity = 10,
                QuestionnaireId = Create.Entity.QuestionnaireIdentity(Id.gA),
                Status = AssignmentStatus.Completed, // supervisor completed it on server
                StatusComment = "Completed by supervisor"
            };

            var syncService = new Mock<ISynchronizationService>();
            syncService.Setup(s => s.GetAssignmentsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AssignmentApiView> { remoteView });

            var synchronizer = Create.Service.AssignmentsSynchronizer(
                synchronizationService: syncService.Object,
                assignmentsRepository: assignmentsRepo
            );

            // Act
            await synchronizer.SynchronizeAssignmentsAsync(Mock.Of<IProgress<SyncProgressInfo>>(), new SynchronizationStatistics(), CancellationToken.None);

            // Assert: server Completed wins over local Finished
            var updated = assignmentsRepo.GetById(1);
            updated.Status.Should().Be(AssignmentStatus.Completed);
            updated.StatusComment.Should().Be("Completed by supervisor");
        }

        [Test]
        public async Task uploads_local_status_change_before_downloading()
        {
            // Arrange: local assignment with Finished status + comment (pending upload indicated by StatusChangedAtUtc)
            var localAssignment = Create.Entity
                .AssignmentDocument(1, 10, 0, Create.Entity.QuestionnaireIdentity(Id.gA).ToString())
                .Build();
            localAssignment.Status = AssignmentStatus.Finished;
            localAssignment.StatusComment = "No more households";
            localAssignment.StatusChangedAtUtc = DateTime.UtcNow;

            var assignmentsRepo = Create.Storage.AssignmentDocumentsInmemoryStorage();
            assignmentsRepo.Store(new[] { localAssignment });

            var remoteView = new AssignmentApiView
            {
                Id = 1,
                Quantity = 10,
                QuestionnaireId = Create.Entity.QuestionnaireIdentity(Id.gA),
                Status = AssignmentStatus.Finished, // server now reflects the change
                StatusComment = "No more households"
            };

            AssignmentStatusChangeApiView capturedChange = null;
            var syncService = new Mock<ISynchronizationService>();
            syncService.Setup(s => s.GetAssignmentsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AssignmentApiView> { remoteView });
            syncService.Setup(s => s.ChangeAssignmentStatusAsync(1, It.IsAny<AssignmentStatusChangeApiView>(), It.IsAny<CancellationToken>()))
                .Callback<int, AssignmentStatusChangeApiView, CancellationToken>((id, change, ct) => capturedChange = change)
                .Returns(Task.CompletedTask);

            var synchronizer = Create.Service.AssignmentsSynchronizer(
                synchronizationService: syncService.Object,
                assignmentsRepository: assignmentsRepo
            );

            // Act
            await synchronizer.SynchronizeAssignmentsAsync(Mock.Of<IProgress<SyncProgressInfo>>(), new SynchronizationStatistics(), CancellationToken.None);

            // Assert: status change was uploaded
            syncService.Verify(s => s.ChangeAssignmentStatusAsync(1, It.IsAny<AssignmentStatusChangeApiView>(), It.IsAny<CancellationToken>()), Times.Once);
            capturedChange.Should().NotBeNull();
            capturedChange.Status.Should().Be(AssignmentStatus.Finished);
            capturedChange.Comment.Should().Be("No more households");

            // After sync, pending flag is cleared
            var updated = assignmentsRepo.GetById(1);
            updated.StatusChangedAtUtc.Should().BeNull();
        }

        [Test]
        public async Task uploads_local_status_change_with_no_comment()
        {
            // Arrange: local assignment with Finished status but no comment (StatusChangedAtUtc set = pending)
            var localAssignment = Create.Entity
                .AssignmentDocument(1, 10, 0, Create.Entity.QuestionnaireIdentity(Id.gA).ToString())
                .Build();
            localAssignment.Status = AssignmentStatus.Finished;
            localAssignment.StatusComment = null;
            localAssignment.StatusChangedAtUtc = DateTime.UtcNow;

            var assignmentsRepo = Create.Storage.AssignmentDocumentsInmemoryStorage();
            assignmentsRepo.Store(new[] { localAssignment });

            var remoteView = new AssignmentApiView
            {
                Id = 1,
                Quantity = 10,
                QuestionnaireId = Create.Entity.QuestionnaireIdentity(Id.gA),
                Status = AssignmentStatus.Finished
            };

            AssignmentStatusChangeApiView capturedChange = null;
            var syncService = new Mock<ISynchronizationService>();
            syncService.Setup(s => s.GetAssignmentsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AssignmentApiView> { remoteView });
            syncService.Setup(s => s.ChangeAssignmentStatusAsync(1, It.IsAny<AssignmentStatusChangeApiView>(), It.IsAny<CancellationToken>()))
                .Callback<int, AssignmentStatusChangeApiView, CancellationToken>((id, change, ct) => capturedChange = change)
                .Returns(Task.CompletedTask);

            var synchronizer = Create.Service.AssignmentsSynchronizer(
                synchronizationService: syncService.Object,
                assignmentsRepository: assignmentsRepo
            );

            // Act
            await synchronizer.SynchronizeAssignmentsAsync(Mock.Of<IProgress<SyncProgressInfo>>(), new SynchronizationStatistics(), CancellationToken.None);

            // Assert: status change was uploaded with null comment
            syncService.Verify(s => s.ChangeAssignmentStatusAsync(1, It.IsAny<AssignmentStatusChangeApiView>(), It.IsAny<CancellationToken>()), Times.Once);
            capturedChange.Should().NotBeNull();
            capturedChange.Status.Should().Be(AssignmentStatus.Finished);
            capturedChange.Comment.Should().BeNull();
        }

        [Test]
        public async Task new_assignment_from_server_gets_server_status_and_comment()
        {
            // Arrange: no local assignments; server returns a Finished assignment with comment
            var assignmentsRepo = Create.Storage.AssignmentDocumentsInmemoryStorage();

            var remoteView = new AssignmentApiView
            {
                Id = 42,
                Quantity = 5,
                QuestionnaireId = Create.Entity.QuestionnaireIdentity(Id.gA),
                Status = AssignmentStatus.Finished,
                StatusComment = "All done"
            };
            // remoteDoc is returned by GetAssignmentAsync — needed to populate assignment answers/details
            var remoteDoc = Create.Entity.AssignmentApiDocument(42, 5, Create.Entity.QuestionnaireIdentity(Id.gA)).Build();

            var questionnaireStorage = new Mock<WB.Core.SharedKernels.DataCollection.Repositories.IQuestionnaireStorage>();
            questionnaireStorage.Setup(s => s.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()))
                .Returns(Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocument()));

            var syncService = new Mock<ISynchronizationService>();
            syncService.Setup(s => s.GetAssignmentsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AssignmentApiView> { remoteView });
            syncService.Setup(s => s.GetAssignmentAsync(42, It.IsAny<CancellationToken>()))
                .ReturnsAsync(remoteDoc);
            syncService.Setup(s => s.LogAssignmentAsHandledAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var synchronizer = Create.Service.AssignmentsSynchronizer(
                synchronizationService: syncService.Object,
                assignmentsRepository: assignmentsRepo,
                questionnaireStorage: questionnaireStorage.Object
            );

            // Act
            await synchronizer.SynchronizeAssignmentsAsync(Mock.Of<IProgress<SyncProgressInfo>>(), new SynchronizationStatistics(), CancellationToken.None);

            // Assert: new assignment has Finished status and comment from server
            var created = assignmentsRepo.GetById(42);
            created.Should().NotBeNull();
            created.Status.Should().Be(AssignmentStatus.Finished);
            created.StatusComment.Should().Be("All done");
        }
    }
}
