using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.OfflineSync
{
    [TestOf(typeof(OfflineSynchronizationService))]
    public class OfflineSynchronizationServiceTests
    {
        [Test]
        public async Task should_always_return_offline_token_when_logged_in()
        {
            var service = Create.Service.OfflineSynchronizationService();

            var token = await service.LoginAsync(new LogonInfo(), new RestCredentials(), CancellationToken.None);

            Assert.That(token, Is.EqualTo("offline sync token"));
        }

        [Test]
        public async Task should_always_bypass_device_link_test()
        {
            var service = Create.Service.OfflineSynchronizationService();

            var hasDevice = await service.HasCurrentUserDeviceAsync();

            Assert.That(hasDevice, Is.True);
        }

        [Test]
        public async Task should_pass_device_build_number_to_can_synchronize_method()
        {
            // arrange
            var appBuldNumber = 12345;
            var mockOfOfflineSyncClient = new Mock<IOfflineSyncClient>();
            mockOfOfflineSyncClient.Setup(x => x.SendAsync<CanSynchronizeRequest, CanSynchronizeResponse>(
                    It.IsAny<CanSynchronizeRequest>(), CancellationToken.None, null))
                .ReturnsAsync(new CanSynchronizeResponse
                {
                    CanSyncronize = true
                });
            var principal = Create.Service.InterviewerPrincipal(Guid.NewGuid());
            var deviceSettings = Mock.Of<IDeviceSettings>(x => x.GetApplicationVersionCode() == appBuldNumber);
            var service = Create.Service.OfflineSynchronizationService(mockOfOfflineSyncClient.Object,
                interviewerPrincipal: principal, deviceSettings: deviceSettings);

            // act
            await service.CanSynchronizeAsync(null, null);

            // assert
            mockOfOfflineSyncClient.Verify(x => x.SendAsync<CanSynchronizeRequest, CanSynchronizeResponse>(
                    It.Is<CanSynchronizeRequest>(y => y.InterviewerBuildNumber == appBuldNumber), It.IsAny<CancellationToken>(), It.IsAny<IProgress<TransferProgress>>()),
                Times.Once);
        }
        
        [Test]
        public async Task CanSynchronize_Should_throw_when_versions_do_not_match()
        {
            var clientMock = new Mock<IOfflineSyncClient>();
            clientMock.Setup(x => x.SendAsync<CanSynchronizeRequest, CanSynchronizeResponse>(
                    It.IsAny<CanSynchronizeRequest>(), CancellationToken.None, null))
                .ReturnsAsync(new CanSynchronizeResponse
                {
                    CanSyncronize = false,
                    Reason = SyncDeclineReason.UnexpectedClientVersion
                });

            var interviewerPrincipal = new Mock<IInterviewerPrincipal>();
            interviewerPrincipal.Setup(x => x.CurrentUserIdentity).Returns(new InterviewerIdentity(){Id = Guid.NewGuid().FormatGuid()});

            var service = Create.Service.OfflineSynchronizationService(clientMock.Object, interviewerPrincipal.Object);

            try
            {
                await service.CanSynchronizeAsync();
            }
            catch (SynchronizationException e)
            {
                Assert.That(e.Type, Is.EqualTo(SynchronizationExceptionType.UpgradeRequired));
            }
        }

        [TestCase(SyncDeclineReason.NotATeamMember, SynchronizationExceptionType.InterviewerFromDifferentTeam)]
        [TestCase(SyncDeclineReason.InvalidPassword, SynchronizationExceptionType.Unauthorized)]
        [TestCase(SyncDeclineReason.UserIsLocked, SynchronizationExceptionType.UserLocked)]
        [TestCase(SyncDeclineReason.SupervisorRequireOnlineSync, SynchronizationExceptionType.SupervisorRequireOnlineSync)]
        [TestCase(SyncDeclineReason.Unknown, SynchronizationExceptionType.Unexpected)]
        public void when_CanSynchronize_and_interviewer_from_different_team_Should_throw_specified_SynchronizationException(
                SyncDeclineReason responseReason, SynchronizationExceptionType syncExceptionType)
        {
            // arrange
            var clientMock = new Mock<IOfflineSyncClient>();
            clientMock.Setup(x => x.SendAsync<CanSynchronizeRequest, CanSynchronizeResponse>(
                    It.IsAny<CanSynchronizeRequest>(), CancellationToken.None, null))
                .ReturnsAsync(new CanSynchronizeResponse
                {
                    CanSyncronize = false,
                    Reason = responseReason
                });

            var interviewerPrincipal = new Mock<IInterviewerPrincipal>();
            interviewerPrincipal.Setup(x => x.CurrentUserIdentity).Returns(new InterviewerIdentity() {Id = Guid.NewGuid().FormatGuid()});

            var service = Create.Service.OfflineSynchronizationService(clientMock.Object, interviewerPrincipal.Object);

            // act
            var e = Assert.CatchAsync<SynchronizationException>(async () => await service.CanSynchronizeAsync());

            // assert
            Assert.That(e.Type, Is.EqualTo(syncExceptionType));
        }
    }
}
