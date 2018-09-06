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
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.SynchronizationProcessTests
{
    [TestOf(typeof(InterviewerOfflineSynchronizationProcess))]
    public class InterviewerOfflineSynchronizationProcessTests
    {
        [Test]
        public void when_synchronize_and_need_to_change_supervisor_should_store_updated_supervisor_id_in_plain_storage()
        {
            InterviewerOfflineSynchronizationProcess viewModel;
            var InterviewerStorageMock = new Mock<IPlainStorage<InterviewerIdentity>>();
            var PrincipalMock = new Mock<IInterviewerPrincipal>();
            var SynchronizationServiceMock = new Mock<IOfflineSynchronizationService>();

            var interviewerIdentity = new InterviewerIdentity { Name = "name", PasswordHash = "hash", Token = "Outdated token", SupervisorId = Id.g1 };

            PrincipalMock = Mock.Get(Setup.InterviewerPrincipal(interviewerIdentity));

            SynchronizationServiceMock
                .Setup(x => x.GetCurrentSupervisor(It.IsAny<CancellationToken>(), It.IsAny<RestCredentials>()))
                .ReturnsAsync(Id.gA);

            InterviewerStorageMock.Setup(x => x.FirstOrDefault()).Returns(interviewerIdentity);

            viewModel = Create.Service.OfflineSynchronizationProcess(principal: PrincipalMock.Object,
                synchronizationService: SynchronizationServiceMock.Object,
                interviewersPlainStorage: InterviewerStorageMock.Object);

            viewModel
                .SynchronizeAsync(new Progress<SyncProgressInfo>(), CancellationToken.None)
                .WaitAndUnwrapException();

            InterviewerStorageMock.Verify(
                x => x.Store(It.Is<InterviewerIdentity>(i => i.SupervisorId == Id.gA)), Times.Once);

            PrincipalMock.Verify(x => x.SignInWithHash("name", "hash", true), Times.Once);
        }

        [Test]
        public async Task when_synchronize_and_need_to_change_password_should_sign_in_user_with_new_credentials()
        {
            var interviewerIdentity = new InterviewerIdentity() { Name = "name", Token = "Outdated token" };

            Mock<IPlainStorage<InterviewerIdentity>> interviewerStorageMock = new Mock<IPlainStorage<InterviewerIdentity>>();
            Mock<IOfflineSynchronizationService> synchronizationServiceMock = new Mock<IOfflineSynchronizationService>();
            Mock<IPasswordHasher> passwordHasherMock = new Mock<IPasswordHasher>();

            var principalMock = Mock.Get(Setup.InterviewerPrincipal(interviewerIdentity));

            synchronizationServiceMock
                .Setup(x => x.LoginAsync(
                    It.IsAny<LogonInfo>(),
                    It.IsAny<RestCredentials>(), null))
                .Returns(Task.FromResult("new token"));

            synchronizationServiceMock
                .Setup(x => x.CanSynchronizeAsync(It.Is<RestCredentials>(r => r.Token == interviewerIdentity.Token), It.IsAny<CancellationToken>()))
                .Throws(new SynchronizationException(type: SynchronizationExceptionType.Unauthorized, message: "Test unauthorized", innerException: null));

            synchronizationServiceMock
                .Setup(x => x.CanSynchronizeAsync(It.Is<RestCredentials>(r => r.Token == "new token"), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            passwordHasherMock
                .Setup(x => x.Hash(It.IsAny<string>()))
                .Returns<string>(x => x);

            passwordHasherMock
                .Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(PasswordVerificationResult.Success);

            interviewerStorageMock
                .Setup(x => x.FirstOrDefault())
                .Returns(interviewerIdentity);

            var syncProcess = Create.Service.OfflineSynchronizationProcess(
                principal: principalMock.Object,
                synchronizationService: synchronizationServiceMock.Object,
                interviewersPlainStorage: interviewerStorageMock.Object,
                passwordHasher: passwordHasherMock.Object);

            // Act
            await syncProcess.SynchronizeAsync(new Progress<SyncProgressInfo>(), CancellationToken.None);

            // Assert
            interviewerStorageMock.Verify(x => x.Store(It.IsAny<InterviewerIdentity>()), Times.Never);
            principalMock.Verify(x => x.SignIn(It.IsAny<string>(), It.IsAny<string>(), true), Times.Never);
        }
    }
}
