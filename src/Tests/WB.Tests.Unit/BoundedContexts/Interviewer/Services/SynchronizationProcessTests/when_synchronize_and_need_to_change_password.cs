using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.HttpServices.HttpClient;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Services.Workspace;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.SynchronizationProcessTests
{
    [NUnit.Framework.TestOf(typeof(InterviewerOnlineSynchronizationProcess))]
    internal class when_synchronize_and_need_to_change_password
    {
        [NUnit.Framework.Test]
        public async Task should_sign_in_user_with_new_credentials()
        {
            var newPassword = "new password";
            var interviewerIdentity = new InterviewerIdentity()
            {
                Name = "name",
                Token = "Outdated token",
                Workspace = "primary"
            };

            Mock<IPlainStorage<InterviewerIdentity>> interviewerStorageMock = new Mock<IPlainStorage<InterviewerIdentity>>();
            Mock<IUserInteractionService> userInteractionServiceMock = new Mock<IUserInteractionService>();
            Mock<IOnlineSynchronizationService> synchronizationServiceMock = new Mock<IOnlineSynchronizationService>();
            Mock<IWorkspaceService> workspaceService = new Mock<IWorkspaceService>();

            Mock<IPasswordHasher> passwordHasherMock = new Mock<IPasswordHasher>();

            var principalMock = Mock.Get(SetUp.InterviewerPrincipal(interviewerIdentity));
            principalMock
                .Setup(x => x.GetInterviewerByName(It.IsAny<string>()))
                .Returns(interviewerIdentity);
            
            userInteractionServiceMock
                .Setup(x => x.ConfirmWithTextInputAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(Task.FromResult(newPassword));

            synchronizationServiceMock
                .Setup(x => x.LoginAsync(
                    It.IsAny<LogonInfo>(),
                    It.IsAny<RestCredentials>(), default))
                .ReturnsAsync("new token");

            synchronizationServiceMock
                .Setup(x => x.CanSynchronizeAsync(It.Is<RestCredentials>(r => r.Token == interviewerIdentity.Token), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new SynchronizationException(type: SynchronizationExceptionType.Unauthorized, message: "Test unauthorized", innerException: null));

            synchronizationServiceMock
                .Setup(x => x.CanSynchronizeAsync(It.Is<RestCredentials>(r => r.Token == "new token"), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            synchronizationServiceMock
                .Setup(x => x.GetInterviewerAsync(It.Is<RestCredentials>(r => r.Token == interviewerIdentity.Token), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new InterviewerApiView() { Workspaces = new List<UserWorkspaceApiView>() { new UserWorkspaceApiView() { Name = "primary"} }}));

            passwordHasherMock
                .Setup(x => x.Hash(It.IsAny<string>()))
                .Returns<string>(x => x);

            passwordHasherMock
                .Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(PasswordVerificationResult.Success);

            interviewerStorageMock
                .Setup(x => x.FirstOrDefault())
                .Returns(interviewerIdentity);

            workspaceService
                .Setup(x => x.GetAll())
                .Returns(new WorkspaceView[] {new WorkspaceView() { Id = "primary" }});

            var viewModel = Create.Service.SynchronizationProcess(principal: principalMock.Object,
                synchronizationService: synchronizationServiceMock.Object,
                userInteractionService: userInteractionServiceMock.Object,
                passwordHasher: passwordHasherMock.Object,
                workspaceService: workspaceService.Object);

            // Act
            await viewModel.SynchronizeAsync(new Progress<SyncProgressInfo>(), default);

            // Assert

            principalMock.Verify(x => x.SaveInterviewer(It.Is<InterviewerIdentity>(i => i.PasswordHash == newPassword)), Times.Once);
            principalMock.Verify(x => x.SaveInterviewer(It.Is<InterviewerIdentity>(i => i.Token == "new token")), Times.Once);
            principalMock.Verify(x => x.SignIn("name", newPassword, true), Times.Once);
        }
    }
}
