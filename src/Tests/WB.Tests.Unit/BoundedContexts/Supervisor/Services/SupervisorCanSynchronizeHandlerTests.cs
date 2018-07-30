using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Supervisor;
using WB.Core.BoundedContexts.Supervisor.Services.Implementation.OfflineSyncHandlers;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Supervisor.Services
{
    [TestOf(typeof(SupervisorCanSynchronizeHandler))]
    public class SupervisorCanSynchronizeHandlerTests
    {
        [Test]
        public async Task CanSynchronize_should_check_assemblyFileVersion_for_compatibility()
        {
            var userId = Guid.NewGuid();
            var userStamp = "test token";
            var users = new Mock<IPlainStorage<InterviewerDocument>>();
            users.Setup(x => x.GetById(userId.FormatGuid())).Returns(new InterviewerDocument() { SecurityStamp = userStamp });

            var handler = Create.Service.SupervisorCanSynchronizeHandler(interviewerViewRepository: users.Object,
                settings: Mock.Of<IEnumeratorSettings>(s => s.LastHqSyncTimestamp == 1));

            var expectedVersion = ReflectionUtils.GetAssemblyVersion(typeof(SupervisorBoundedContextAssemblyIndicator));
            var response = await handler.CanSynchronize(new CanSynchronizeRequest(expectedVersion.Revision, userId, userStamp, null));

            Assert.That(response, Has.Property(nameof(response.CanSyncronize)).True);
        }

        [Test]
        [Ignore("KP-11677")]
        public async Task CanSynchronize_should_check_security_stamp()
        {
            var userId = Guid.NewGuid();
            var userToken = "test token";
            var users = new Mock<IPlainStorage<InterviewerDocument>>();
            users.Setup(x => x.GetById(userId.FormatGuid())).Returns(new InterviewerDocument() { SecurityStamp = userToken });

            var handler = Create.Service.SupervisorCanSynchronizeHandler(interviewerViewRepository: users.Object,
                settings: Mock.Of<IEnumeratorSettings>(s => s.LastHqSyncTimestamp == 1));

            var expectedVersion = ReflectionUtils.GetAssemblyVersion(typeof(SupervisorBoundedContextAssemblyIndicator));
            var response = await handler.CanSynchronize(new CanSynchronizeRequest(expectedVersion.Revision, userId, "new token", null));

            Assert.That(response, Has.Property(nameof(response.CanSyncronize)).False);
            Assert.AreEqual(response.Reason, SyncDeclineReason.InvalidPassword);
        }

        [Test]
        public async Task CanSynchronize_should_check_hq_timestamp_do_not_allow_offline_sync_if_sv_sync_outdated()
        {
            const long interviewerLastHqTimestamp = 10;
            const long supervisorLastHqTimestamp = 5;

            var userId = Guid.NewGuid();
            var users = new Mock<IPlainStorage<InterviewerDocument>>();
            users.Setup(x => x.GetById(userId.FormatGuid())).Returns(new InterviewerDocument());

            var handler = Create.Service.SupervisorCanSynchronizeHandler(interviewerViewRepository: users.Object,
                settings: Mock.Of<IEnumeratorSettings>(s => s.LastHqSyncTimestamp == supervisorLastHqTimestamp));

            var expectedVersion = ReflectionUtils.GetAssemblyVersion(typeof(SupervisorBoundedContextAssemblyIndicator));
            var response = await handler.CanSynchronize(
                new CanSynchronizeRequest(expectedVersion.Revision, userId, String.Empty, interviewerLastHqTimestamp));

            Assert.That(response, Has.Property(nameof(response.CanSyncronize)).False);
            Assert.AreEqual(response.Reason, SyncDeclineReason.SupervisorRequireOnlineSync);
        }

        [Test]
        public async Task CanSynchronize_should_check_hq_timestamp_do_not_allow_offline_sync_if_no_online_occur()
        {
            var userId = Guid.NewGuid();
            var userToken = "test token";
            var users = new Mock<IPlainStorage<InterviewerDocument>>();
            users.Setup(x => x.GetById(userId.FormatGuid())).Returns(new InterviewerDocument() { SecurityStamp = userToken });

            var handler = Create.Service.SupervisorCanSynchronizeHandler(interviewerViewRepository: users.Object,
                settings: Mock.Of<IEnumeratorSettings>(s => s.LastHqSyncTimestamp == null));

            var expectedVersion = ReflectionUtils.GetAssemblyVersion(typeof(SupervisorBoundedContextAssemblyIndicator));
            var response = await handler.CanSynchronize(new CanSynchronizeRequest(expectedVersion.Revision, userId, "new token", null));

            Assert.That(response, Has.Property(nameof(response.CanSyncronize)).False);
            Assert.AreEqual(response.Reason, SyncDeclineReason.SupervisorRequireOnlineSync);
        }

        [Test]
        public async Task CanSynchronize_should_check_UserId_for_Team_belonging()
        {
            var userId = Guid.NewGuid();
            var users = new Mock<IPlainStorage<InterviewerDocument>>();
            users.Setup(x => x.GetById(userId.FormatGuid())).Returns(new InterviewerDocument());

            var handler = Create.Service.SupervisorCanSynchronizeHandler(interviewerViewRepository: users.Object,
                settings: Mock.Of<IEnumeratorSettings>(s => s.LastHqSyncTimestamp == 1));

            var expectedVersion = ReflectionUtils.GetAssemblyVersion(typeof(SupervisorBoundedContextAssemblyIndicator));
            var response = await handler.CanSynchronize(new CanSynchronizeRequest(expectedVersion.Revision, Guid.NewGuid(), String.Empty, null));

            Assert.That(response, Has.Property(nameof(response.CanSyncronize)).False);
            Assert.AreEqual(response.Reason, SyncDeclineReason.NotATeamMember);
        }

        [Test]
        public async Task CanSynchronize_should_check_assemblyFileVersion_for_incompatibility()
        {
            var handler = Create.Service.SupervisorCanSynchronizeHandler();

            var response = await handler.CanSynchronize(new CanSynchronizeRequest(1, Guid.NewGuid(), String.Empty, null));

            Assert.That(response, Has.Property(nameof(response.CanSyncronize)).False);
        }
    }
}