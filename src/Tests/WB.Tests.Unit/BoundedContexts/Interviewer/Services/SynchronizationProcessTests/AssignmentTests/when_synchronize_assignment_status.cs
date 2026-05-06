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
            // Arrange: local has Finished, server returns Active
            var localAssignment = Create.Entity
                .AssignmentDocument(1, 10, 0, Create.Entity.QuestionnaireIdentity(Id.gA).ToString())
                .Build();
            localAssignment.Status = AssignmentStatus.Finished;
            localAssignment.StatusComment = "some comment";

            var assignmentsRepo = Create.Storage.AssignmentDocumentsInmemoryStorage();
            assignmentsRepo.Store(new[] { localAssignment });

            var remoteView = new AssignmentApiView
            {
                Id = 1,
                Quantity = 10,
                QuestionnaireId = Create.Entity.QuestionnaireIdentity(Id.gA),
                Status = AssignmentStatus.Active // server has Active
            };

            var syncService = new Mock<ISynchronizationService>();
            syncService.Setup(s => s.GetAssignmentsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AssignmentApiView> { remoteView });
            syncService.Setup(s => s.ChangeAssignmentStatusAsync(It.IsAny<int>(), It.IsAny<AssignmentStatusChangeApiView>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var synchronizer = Create.Service.AssignmentsSynchronizer(
                synchronizationService: syncService.Object,
                assignmentsRepository: assignmentsRepo
            );

            // Act
            await synchronizer.SynchronizeAssignmentsAsync(Mock.Of<IProgress<SyncProgressInfo>>(), new SynchronizationStatistics(), CancellationToken.None);

            // Assert: local status should be overridden to Active by server
            var updated = assignmentsRepo.GetById(1);
            updated.Status.Should().Be(AssignmentStatus.Active);
            updated.StatusComment.Should().BeNull("status comment should be cleared after successful sync");
        }

        [Test]
        public async Task server_completed_status_overrides_local_finished()
        {
            // Arrange: local has Finished, server has Completed (e.g., supervisor completed it)
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
                Status = AssignmentStatus.Completed // supervisor completed it on server
            };

            var syncService = new Mock<ISynchronizationService>();
            syncService.Setup(s => s.GetAssignmentsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AssignmentApiView> { remoteView });
            syncService.Setup(s => s.ChangeAssignmentStatusAsync(It.IsAny<int>(), It.IsAny<AssignmentStatusChangeApiView>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var synchronizer = Create.Service.AssignmentsSynchronizer(
                synchronizationService: syncService.Object,
                assignmentsRepository: assignmentsRepo
            );

            // Act
            await synchronizer.SynchronizeAssignmentsAsync(Mock.Of<IProgress<SyncProgressInfo>>(), new SynchronizationStatistics(), CancellationToken.None);

            // Assert: server Completed wins over local Finished
            var updated = assignmentsRepo.GetById(1);
            updated.Status.Should().Be(AssignmentStatus.Completed);
        }

        [Test]
        public async Task uploads_local_status_change_before_downloading()
        {
            // Arrange: local assignment with Finished status + comment
            var localAssignment = Create.Entity
                .AssignmentDocument(1, 10, 0, Create.Entity.QuestionnaireIdentity(Id.gA).ToString())
                .Build();
            localAssignment.Status = AssignmentStatus.Finished;
            // StatusComment is set to non-null to signal a pending upload (empty string = pending, no comment)
            localAssignment.StatusComment = "No more households";

            var assignmentsRepo = Create.Storage.AssignmentDocumentsInmemoryStorage();
            assignmentsRepo.Store(new[] { localAssignment });

            var remoteView = new AssignmentApiView
            {
                Id = 1,
                Quantity = 10,
                QuestionnaireId = Create.Entity.QuestionnaireIdentity(Id.gA),
                Status = AssignmentStatus.Finished // server now reflects the change
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
        }

        [Test]
        public async Task uploads_local_status_change_with_no_comment()
        {
            // Arrange: local assignment with Finished status but no comment
            var localAssignment = Create.Entity
                .AssignmentDocument(1, 10, 0, Create.Entity.QuestionnaireIdentity(Id.gA).ToString())
                .Build();
            localAssignment.Status = AssignmentStatus.Finished;
            // Empty string signals "change pending, but no comment was entered"
            localAssignment.StatusComment = string.Empty;

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

            // Assert: status change was uploaded with null comment (empty string normalized to null)
            syncService.Verify(s => s.ChangeAssignmentStatusAsync(1, It.IsAny<AssignmentStatusChangeApiView>(), It.IsAny<CancellationToken>()), Times.Once);
            capturedChange.Should().NotBeNull();
            capturedChange.Status.Should().Be(AssignmentStatus.Finished);
            capturedChange.Comment.Should().BeNull("empty string comment is normalized to null before sending to server");
        }

        [Test]
        public async Task new_assignment_from_server_gets_server_status()
        {
            // Arrange: no local assignments; server returns a Finished assignment
            var assignmentsRepo = Create.Storage.AssignmentDocumentsInmemoryStorage();

            var remoteView = new AssignmentApiView
            {
                Id = 42,
                Quantity = 5,
                QuestionnaireId = Create.Entity.QuestionnaireIdentity(Id.gA),
                Status = AssignmentStatus.Finished
            };
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

            // Assert: new assignment has Finished status from server
            var created = assignmentsRepo.GetById(42);
            created.Should().NotBeNull();
            created.Status.Should().Be(AssignmentStatus.Finished);
        }
    }
}
