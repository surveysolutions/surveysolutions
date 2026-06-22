using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.BoundedContexts.Supervisor.ViewModel;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Supervisor.ViewModels.LoginViewModelTests
{
    [TestOf(typeof(LoginViewModel))]
    public class when_singing_in_with_hash_and_user_exist
    {
        [OneTimeSetUp]
        public async Task context()
        {
            var passwordHasher = Mock.Of<IPasswordHasher>();
            var storage = Create.Storage.InMemorySqlitePlainStorage<SupervisorIdentity>();
            var supervisor = Create.Other.SupervisorIdentity(userName: userName, passwordHash: userPasswordHash);
            storage.Store(supervisor);

            var principal = new Mock<IPrincipal>();
            principal.Setup(x => x.SignInWithHash(It.IsAny<string>(), userPasswordHash, true)).Returns(true);

            ViewModelNavigationServiceMock
                .Setup(x => x.NavigateToDashboardAsync(null))
                .Returns(Task.FromResult(true));
            ViewModelNavigationServiceMock
                .Setup(x => x.Close(It.IsAny<LoginViewModel>()))
                .Returns(Task.CompletedTask);

            viewModel = new LoginViewModel(
                ViewModelNavigationServiceMock.Object,
                principal.Object,
                passwordHasher,
                storage,
                Mock.Of<ICompanyLogoStorage>(),
                Mock.Of<ISynchronizationService>(),
                Mock.Of<ILogger>(),
                Mock.Of<IAuditLogService>());

            await viewModel.Initialize();
            viewModel.UserName = userName.ToUpperInvariant();
            viewModel.Password = "entered-password";
            await viewModel.SignInWithHashCommand.ExecuteAsync();
        }

        [Test]
        public void should_get_user_password_hash_by_username()
            => Assert.That(viewModel.GetUserPasswordHash(), Is.EqualTo(userPasswordHash));

        [Test]
        public void should_navigate_to_dashboard()
            => ViewModelNavigationServiceMock.Verify(x => x.NavigateToDashboardAsync(null), Times.Once);

        [Test]
        public void should_close_login_viewmodel()
            => ViewModelNavigationServiceMock.Verify(x => x.Close(viewModel), Times.Once);

        static LoginViewModel viewModel;
        private static readonly string userName = "supervisor";
        private static readonly string userPasswordHash = "passwordHash";
        static Mock<IViewModelNavigationService> ViewModelNavigationServiceMock = new Mock<IViewModelNavigationService>();
    }
}
