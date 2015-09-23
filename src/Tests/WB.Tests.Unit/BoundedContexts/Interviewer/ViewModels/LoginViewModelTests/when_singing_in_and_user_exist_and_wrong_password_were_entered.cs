using Machine.Specifications;

using Moq;

using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.ViewModels.LoginViewModelTests
{
    public class when_singing_in_and_user_exist_and_wrong_password_were_entered : LoginViewModelTestContext
    {
        Establish context = () =>
        {
            var passwordHasher = Mock.Of<IPasswordHasher>(x => x.Hash(userPassword) == userPasswordHash);

            var interviewer = CreateInterviewerIdentity(userName, userPasswordHash);

            var principal = new Mock<IPrincipal>();
            principal.Setup(x => x.SignIn(userName, userPasswordHash, true)).Returns(true);
            principal.Setup(x => x.SignIn(userName, wrongPassword, true)).Returns(false);

            viewModel = CreateLoginViewModel(
                interviewer: interviewer,
                passwordHasher: passwordHasher,
                principal: principal.Object);

            viewModel.Init();
            viewModel.Password = wrongPassword;
        };

        Because of = () => viewModel.SignInCommand.Execute();

        It should_not_navigate_to_dashboard = () =>
            ViewModelNavigationServiceMock.Verify(x => x.NavigateToDashboard(), Times.Never);

        It should_not_show_online_login_button = () =>
            viewModel.IsOnlineLoginButtonVisible.ShouldBeFalse();

        It should_set_Login_Online_Signin_Explanation_message = () =>
            viewModel.ErrorMessage.ShouldEqual(InterviewerUIResources.Login_WrondPassword);

        static LoginViewModel viewModel;
        private static readonly string userName = "Vasya";
        private static readonly string userPassword = "password";
        private static readonly string userPasswordHash = "passwordHash";
        private static readonly string wrongPassword = "wrongPassword";
    }
}