using System;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Services;
using Web.Supervisor.Controllers;
using Web.Supervisor.Models;
using It = Machine.Specifications.It;

namespace Web.Supervisor.Tests.AccountControllerTests
{
    internal class when_logging_in_user_and_credentials_are_invalid : AccountControllerTestsContext
    {
        Establish context = () =>
        {
            Func<string, string, bool> validateUserCredentials = (login, passwordHash) => false;

            headquartersSynchronizerMock = new Mock<IHeadquartersSynchronizer>();

            controller = CreateAccountController(
                headquartersSynchronizer: headquartersSynchronizerMock.Object,
                validateUserCredentials: validateUserCredentials);
        };

        Because of = () =>
            controller.LogOn(new LogOnModel { UserName = login, Password = password }, string.Empty);

        It should_pull_data_from_headquarters_using_specified_credentials = () =>
            headquartersSynchronizerMock.Verify(_ => _.Pull(login, password), Times.Once);

        private static Mock<IHeadquartersSynchronizer> headquartersSynchronizerMock;
        private static string login = "login";
        private static string password = "pwd";
        private static AccountController controller;
    }
}