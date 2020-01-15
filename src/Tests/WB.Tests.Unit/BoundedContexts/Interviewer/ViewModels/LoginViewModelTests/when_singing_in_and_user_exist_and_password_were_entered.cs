using System.Threading.Tasks;
using Moq;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.ViewModels.LoginViewModelTests
{
    public class when_singing_in_and_user_exist_and_password_were_entered : LoginViewModelTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public async Task context () {
            var passwordHasher = Mock.Of<IPasswordHasher>(x => x.Hash(userPassword) == userPasswordHash);

            var interviewer = CreateInterviewerIdentity(userName, userPasswordHash);

            var principal = new Mock<IInterviewerPrincipal>();
            principal.Setup(x => x.SignIn(userName, userPassword, true)).Returns(true);
            principal
                .Setup(x => x.DoesIdentityExist())
                .Returns(true);

            principal
                .Setup(x => x.GetExistingIdentityNameOrNull())
                .Returns(interviewer.Name);


            viewModel = CreateLoginViewModel(
                viewModelNavigationService: ViewModelNavigationServiceMock.Object,
                passwordHasher: passwordHasher,
                principal: principal.Object);

            await viewModel.Initialize();
            viewModel.UserName = userName;
            viewModel.Password = userPassword;
            BecauseOf();
        }

        public void BecauseOf() => viewModel.SignInCommand.Execute();

        [NUnit.Framework.Test] public void should_navigate_to_dashboard () =>
            ViewModelNavigationServiceMock.Verify(x => x.NavigateToDashboardAsync(null), Times.Once);

        static LoginViewModel viewModel;
        private static readonly string userName = "Vasya";
        private static readonly string userPassword = "password";
        private static readonly string userPasswordHash = "passwordHash";
        static Mock<IViewModelNavigationService> ViewModelNavigationServiceMock = new Mock<IViewModelNavigationService>();
    }
}
