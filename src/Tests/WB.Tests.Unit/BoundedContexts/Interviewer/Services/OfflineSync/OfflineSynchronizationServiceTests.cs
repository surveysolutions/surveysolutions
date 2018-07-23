using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.OfflineSync
{
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
            var v = ReflectionUtils.GetAssemblyVersion(typeof(InterviewerBoundedContextAssemblyIndicator));

            var clientMock = new Mock<IOfflineSyncClient>();
            clientMock.Setup(x => x.SendAsync<CanSynchronizeRequest, CanSynchronizeResponse>(
                    It.Is<CanSynchronizeRequest>(r => r.InterviewerBuildNumber == v.Revision), CancellationToken.None, null))
                .ReturnsAsync(new CanSynchronizeResponse
                {
                    CanSyncronize = true
                });

            var interviewerPrincipal = new Mock<IInterviewerPrincipal>();
            interviewerPrincipal.Setup(x => x.CurrentUserIdentity).Returns(new InterviewerIdentity() { Id = Guid.NewGuid().FormatGuid() });

            var service = Create.Service.OfflineSynchronizationService(clientMock.Object, interviewerPrincipal.Object);

            await service.CanSynchronizeAsync(null, null);
        }
        
        [Test]
        public async Task CanSynchronize_Should_throw_when_versions_do_not_match()
        {
            var v = ReflectionUtils.GetAssemblyVersion(typeof(InterviewerBoundedContextAssemblyIndicator));

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
    }
}
