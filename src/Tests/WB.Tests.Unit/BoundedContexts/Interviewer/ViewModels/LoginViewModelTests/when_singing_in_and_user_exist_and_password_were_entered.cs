using System;
using System.Linq;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.ViewModels.LoginViewModelTests
{
    public class when_singing_in_and_user_exist_and_password_were_entered : LoginViewModelTestContext
    {
        Establish context = () =>
        {
            var passwordHasher = Mock.Of<IPasswordHasher>(x => x.Hash(userPassword) == userPasswordHash);

            var interviewer = CreateInterviewerIdentity(userName, userPasswordHash);

            var principal = new Mock<IPrincipal>();
            principal.Setup(x => x.SignInAsync(userName, userPasswordHash, true)).Returns(Task.FromResult(true));

            InterviewersPlainStorageMock
               .Setup(x => x.FirstOrDefault())
               .Returns(interviewer);

            viewModel = CreateLoginViewModel(
                viewModelNavigationService: ViewModelNavigationServiceMock.Object,
                interviewersPlainStorage: InterviewersPlainStorageMock.Object,
                passwordHasher: passwordHasher,
                principal: principal.Object);

            viewModel.Init();
            viewModel.Password = userPassword;
        };

        Because of = () => viewModel.SignInCommand.Execute();

        It should_navigate_to_dashboard = () =>
            ViewModelNavigationServiceMock.Verify(x => x.NavigateToDashboardAsync(), Times.Once);

        static LoginViewModel viewModel;
        private static readonly string userName = "Vasya";
        private static readonly string userPassword = "password";
        private static readonly string userPasswordHash = "passwordHash";
        static Mock<IViewModelNavigationService> ViewModelNavigationServiceMock = new Mock<IViewModelNavigationService>();
        static Mock<IAsyncPlainStorage<InterviewerIdentity>> InterviewersPlainStorageMock = new Mock<IAsyncPlainStorage<InterviewerIdentity>>();
    }
}