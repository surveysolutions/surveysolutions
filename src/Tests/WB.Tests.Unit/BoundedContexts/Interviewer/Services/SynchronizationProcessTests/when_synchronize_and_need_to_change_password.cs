using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
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
            var interviewerIdentity = new InterviewerIdentity() { Name = "name", Token = "Outdated token" };

            Mock<IPlainStorage<InterviewerIdentity>> interviewerStorageMock = new Mock<IPlainStorage<InterviewerIdentity>>();
            Mock<IUserInteractionService> userInteractionServiceMock = new Mock<IUserInteractionService>();
            Mock<IOnlineSynchronizationService> synchronizationServiceMock = new Mock<IOnlineSynchronizationService>();

            Mock<IPasswordHasher> passwordHasherMock = new Mock<IPasswordHasher>();

            var principalMock = Mock.Get(SetUp.InterviewerPrincipal(interviewerIdentity));
            principalMock.Setup(x => x.GetInterviewerByName(It.IsAny<string>())).Returns(interviewerIdentity);
            
            userInteractionServiceMock
                .Setup(x => x.ConfirmWithTextInputAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(Task.FromResult(newPassword));

            synchronizationServiceMock
                .Setup(x => x.LoginAsync(
                    It.IsAny<LogonInfo>(),
                    It.IsAny<RestCredentials>(), default))
                .Returns(Task.FromResult("new token"));

            synchronizationServiceMock
                .Setup(x => x.CanSynchronizeAsync(It.Is<RestCredentials>(r => r.Token == interviewerIdentity.Token), null, It.IsAny<CancellationToken>()))
                .Throws(new SynchronizationException(type: SynchronizationExceptionType.Unauthorized, message: "Test unauthorized", innerException: null));

            synchronizationServiceMock
                .Setup(x => x.CanSynchronizeAsync(It.Is<RestCredentials>(r => r.Token == "new token"), null, It.IsAny<CancellationToken>()))
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

            var viewModel = Create.Service.SynchronizationProcess(principal: principalMock.Object,
                synchronizationService: synchronizationServiceMock.Object,
                userInteractionService: userInteractionServiceMock.Object,
                passwordHasher: passwordHasherMock.Object);

            // Act
            await viewModel.SynchronizeAsync(new Progress<SyncProgressInfo>(), CancellationToken.None);

            // Assert

            principalMock.Verify(x => x.SaveInterviewer(It.Is<InterviewerIdentity>(i => i.PasswordHash == newPassword)), Times.Once);
            principalMock.Verify(x => x.SaveInterviewer(It.Is<InterviewerIdentity>(i => i.Token == "new token")), Times.Once);
            principalMock.Verify(x => x.SignIn("name", newPassword, true), Times.Once);
        }
    }
}
