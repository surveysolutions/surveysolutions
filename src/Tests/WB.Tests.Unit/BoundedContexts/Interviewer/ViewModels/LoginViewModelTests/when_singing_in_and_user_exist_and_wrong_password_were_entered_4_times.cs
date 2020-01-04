using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.ViewModels.LoginViewModelTests
{
    public class when_singing_in_and_user_exist_and_wrong_password_were_entered_4_times : LoginViewModelTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public async Task context () {
            var passwordHasher = Mock.Of<IPasswordHasher>(x => x.Hash(userPassword) == userPasswordHash);

            var interviewer = CreateInterviewerIdentity(userName, userPasswordHash);

            var principal = new Mock<IInterviewerPrincipal>();
            principal.Setup(x => x.SignIn(userName, userPasswordHash, true)).Returns(true);
            principal.Setup(x => x.SignIn(userName, wrongPassword, true)).Returns(false);
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
            viewModel.Password = wrongPassword;

            viewModel.SignInCommand.Execute();
            viewModel.SignInCommand.Execute();
            viewModel.SignInCommand.Execute();
            viewModel.SignInCommand.Execute();
            BecauseOf();
        }

        public void BecauseOf() => viewModel.SignInCommand.Execute();

        [NUnit.Framework.Test] public void should_not_navigate_to_dashboard () =>
            ViewModelNavigationServiceMock.Verify(x => x.NavigateToDashboardAsync(null), Times.Never);

        [NUnit.Framework.Test] public void should_show_online_login_button () =>
            viewModel.IsOnlineLoginButtonVisible.Should().BeTrue();

        [NUnit.Framework.Test] public void should_set_Login_Online_Signin_Explanation_message () =>
            viewModel.ErrorMessage.Should().Be(EnumeratorUIResources.Login_Online_Signin_Explanation_message);

        static LoginViewModel viewModel;
        private static readonly string userName = "Vasya";
        private static readonly string userPassword = "password";
        private static readonly string userPasswordHash = "passwordHash";

        private static readonly string wrongPassword = "wrongPassword";
        static Mock<IViewModelNavigationService> ViewModelNavigationServiceMock = new Mock<IViewModelNavigationService>();
    }
}
