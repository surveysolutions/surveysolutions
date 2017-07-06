using System;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Tests.Abc;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.SynchronizationProcessTests
{
    [Subject(typeof(SynchronizationProcess))]
    internal class when_synchronize_and_need_to_change_password
    {
        private Establish context = () =>
        {
            var interviewerIdentity = new InterviewerIdentity() {Name = "name", Token = "Outdated token"};

            principalMock
                .Setup(x => x.CurrentUserIdentity)
                .Returns(interviewerIdentity);

            userInteractionServiceMock
                .Setup(x => x.ConfirmWithTextInputAsync(Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<bool>()))
                .Returns(Task.FromResult("new password"));

            synchronizationServiceMock
                .Setup(x => x.LoginAsync(
                    Moq.It.IsAny<LogonInfo>(),
                    Moq.It.IsAny<RestCredentials>(), null))
                .Returns(Task.FromResult("new token"));

            synchronizationServiceMock
                .Setup(x => x.CanSynchronizeAsync(Moq.It.Is<RestCredentials>(r=>r.Password== interviewerIdentity.Password), Moq.It.IsAny<CancellationToken>()))
                .Throws(new SynchronizationException(type: SynchronizationExceptionType.Unauthorized, message: "", innerException: null));
            
            passwordHasherMock
                .Setup(x => x.Hash(Moq.It.IsAny<string>()))
                .Returns<string>(x => x);

            passwordHasherMock
                .Setup(x => x.VerifyPassword(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Returns<bool>(x => true);

            interviewerStorageMock
                .Setup(x => x.FirstOrDefault())
                .Returns(interviewerIdentity);

            viewModel = Create.Service.SynchronizationProcess(principal: principalMock.Object,
                synchronizationService: synchronizationServiceMock.Object,
                interviewersPlainStorage: interviewerStorageMock.Object,
                userInteractionService: userInteractionServiceMock.Object,
                passwordHasher: passwordHasherMock.Object);
        };

        Because of = () => viewModel.SyncronizeAsync(new Progress<SyncProgressInfo>(), CancellationToken.None).WaitAndUnwrapException();

        It should_store_updated_user_password_in_plain_storage = () =>
            interviewerStorageMock.Verify(
                x => x.Store(Moq.It.Is<InterviewerIdentity>(i => i.PasswordHash == "new password")), Times.Once);

        It should_store_updated_user_token_in_plain_storage = () =>
            interviewerStorageMock.Verify(
                x => x.Store(Moq.It.Is<InterviewerIdentity>(i => i.Token == "new token")), Times.Once);

        It should_sign_in_user_with_new_credentials = () =>
           principalMock.Verify(x => x.SignIn("name", "new password", true), Times.Once);

        static SynchronizationProcess viewModel;
        static Mock<IPlainStorage<InterviewerIdentity>> interviewerStorageMock = new Mock<IPlainStorage<InterviewerIdentity>>();
        static Mock<IUserInteractionService> userInteractionServiceMock=new Mock<IUserInteractionService>();
        static Mock<IPrincipal> principalMock = new Mock<IPrincipal>();
        static Mock<ISynchronizationService>  synchronizationServiceMock =new Mock<ISynchronizationService>();
        static Mock<IPasswordHasher> passwordHasherMock = new Mock<IPasswordHasher>();
    }
}