using System.Threading.Tasks;
using Moq;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.ViewModels.LoginViewModelTests
{
    public class when_singing_in_remotly_and_user_exist_and_new_password_were_entered : LoginViewModelTestContext
    {
        [NUnit.Framework.OneTimeSetUp]
        public async Task context()
        {
            var passwordHasher = Mock.Of<IPasswordHasher>(x => x.Hash(newUserPassword) == userPasswordHash);

            var interviewer = CreateInterviewerIdentity(userName, userPasswordHash);

            var principal = new Mock<IPrincipal>();
            principal.Setup(x => x.SignIn(userName, newUserPassword, true)).Returns(true);

            InterviewersPlainStorageMock
               .Setup(x => x.FirstOrDefault())
               .Returns(interviewer);

            synchronizationServiceMock
              .Setup(x => x.LoginAsync(
                  Moq.It.IsAny<LogonInfo>(),
                  Moq.It.IsAny<RestCredentials>(), null))
              .Returns(Task.FromResult(userToken));

            viewModel = CreateLoginViewModel(
                viewModelNavigationService: ViewModelNavigationServiceMock.Object,
                interviewersPlainStorage: InterviewersPlainStorageMock.Object,
                passwordHasher: passwordHasher,
                synchronizationService: synchronizationServiceMock.Object,
                principal: principal.Object);

            await viewModel.Initialize();
            viewModel.Password = newUserPassword;

            // Act
            await viewModel.OnlineSignInCommand.ExecuteAsync();
        }

        [NUnit.Framework.Test]
        public void should_navigate_to_dashboard() =>
            ViewModelNavigationServiceMock.Verify(x => x.NavigateToDashboardAsync(null), Times.Once);

        [NUnit.Framework.Test]
        public void should_store_entered_password() =>
           InterviewersPlainStorageMock.Verify(x => x.Store(Moq.It.Is<InterviewerIdentity>(i => i.PasswordHash == userPasswordHash)), Times.Once);

        [NUnit.Framework.Test]
        public void should_store_token_from_login() =>
           InterviewersPlainStorageMock.Verify(x => x.Store(Moq.It.Is<InterviewerIdentity>(i => i.Token == userToken)), Times.Once);

        [NUnit.Framework.Test]
        public void should_login_remotly() =>
            synchronizationServiceMock.Verify(x => x.LoginAsync(Moq.It.Is<LogonInfo>(li =>
                    li.Username == userName && li.Password == newUserPassword), Moq.It.IsAny<RestCredentials>(), null), Times.Once);

        static LoginViewModel viewModel;
        private static readonly string userName = "Vasya";
        private static readonly string newUserPassword = "newPassword";
        private static readonly string userPasswordHash = "passwordHash";
        private static readonly string userToken = "new token";

        static Mock<ISynchronizationService> synchronizationServiceMock = new Mock<ISynchronizationService>();
        static Mock<IViewModelNavigationService> ViewModelNavigationServiceMock = new Mock<IViewModelNavigationService>();
        static Mock<IPlainStorage<InterviewerIdentity>> InterviewersPlainStorageMock = new Mock<IPlainStorage<InterviewerIdentity>>();
    }
}
