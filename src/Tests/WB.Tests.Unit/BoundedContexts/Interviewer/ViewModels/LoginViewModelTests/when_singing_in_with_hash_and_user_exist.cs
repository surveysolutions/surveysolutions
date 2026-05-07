using System.Threading.Tasks;
using Moq;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.ViewModels.LoginViewModelTests
{
    public class when_singing_in_with_hash_and_user_exist : LoginViewModelTestContext
    {
        [NUnit.Framework.OneTimeSetUp]
        public async Task context()
        {
            var passwordHasher = Mock.Of<IPasswordHasher>();

            var interviewer = CreateInterviewerIdentity(userName, userPasswordHash);

            var principal = new Mock<IInterviewerPrincipal>();
            principal.Setup(x => x.SignInWithHash(userName, userPasswordHash, true)).Returns(true);
            principal.Setup(x => x.DoesIdentityExist()).Returns(true);
            principal.Setup(x => x.GetExistingIdentityNameOrNull()).Returns(interviewer.Name);
            principal.Setup(x => x.GetInterviewerByName(userName)).Returns(interviewer);

            viewModel = CreateLoginViewModel(
                viewModelNavigationService: ViewModelNavigationServiceMock.Object,
                passwordHasher: passwordHasher,
                principal: principal.Object);

            await viewModel.Initialize();
            viewModel.UserName = userName;
            BecauseOf();
        }

        public void BecauseOf() => viewModel.SignInWithHashCommand.Execute();

        [NUnit.Framework.Test] public void should_navigate_to_dashboard() =>
            ViewModelNavigationServiceMock.Verify(x => x.NavigateToDashboardAsync(null), Times.Once);

        static LoginViewModel viewModel;
        private static readonly string userName = "Vasya";
        private static readonly string userPasswordHash = "passwordHash";
        static Mock<IViewModelNavigationService> ViewModelNavigationServiceMock = new Mock<IViewModelNavigationService>();
    }
}
