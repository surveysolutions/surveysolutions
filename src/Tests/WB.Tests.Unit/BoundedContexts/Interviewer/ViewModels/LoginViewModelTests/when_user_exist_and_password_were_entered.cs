using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.ViewModels.LoginViewModelTests
{
    public class when_user_exist_and_password_were_entered : LoginViewModelTestContext
    {
        Establish context = () =>
        {
            var passwordHasher = Mock.Of<IPasswordHasher>(x => x.Hash(userPassword) == userPasswordHash);

            var interviewer = CreateInterviewerIdentity(userName, userPasswordHash);

            var principal = new Mock<IPrincipal>();
            principal.Setup(x => x.SignIn(userName, userPasswordHash, true)).Returns(true);

            viewModel = CreateLoginViewModel(
                interviewer: interviewer,
                passwordHasher: passwordHasher,
                principal: principal.Object,
                viewModelNavigationService: ViewModelNavigationService.Object);

            viewModel.Init();
            viewModel.Password = userPassword;
        };

        Because of = () => viewModel.SignInCommand.Execute();

        It should_navigate_to_dashboard = () =>
            ViewModelNavigationService.Verify(x => x.NavigateToDashboard(), Times.Once);

        static LoginViewModel viewModel;
        private static readonly string userName = "Vasya";
        private static readonly string userPassword = "password";
        private static readonly string userPasswordHash = "passwordHash";

        private static readonly Mock<IViewModelNavigationService> ViewModelNavigationService = new Mock<IViewModelNavigationService>();
    }
}