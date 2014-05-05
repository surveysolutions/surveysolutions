using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Supervisor.Users;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.UI.Supervisor.Controllers;
using WB.UI.Supervisor.Models;
using It = Machine.Specifications.It;

namespace WB.UI.Supervisor.Tests.AccountControllerTests
{
    internal class when_logging_in_user_and_credentials_are_invalid : AccountControllerTestsContext
    {
        Establish context = () =>
        {
            Func<string, string, bool> validateUserCredentials = (login, passwordHash) => false;

            loginServiceMock = new Mock<IHeadquartersLoginService>();

            controller = CreateAccountController(
                loginService: loginServiceMock.Object,
                validateUserCredentials: validateUserCredentials);
        };

        Because of = () =>
            controller.LogOn(new LogOnModel { UserName = login, Password = password }, string.Empty);

        It should_pull_data_from_headquarters_using_specified_credentials = () =>
            loginServiceMock.Verify(_ => _.LoginAndCreateAccount(login, password), Times.Once);

        private static Mock<IHeadquartersLoginService> loginServiceMock;
        private static string login = "login";
        private static string password = "pwd";
        private static AccountController controller;
    }
}