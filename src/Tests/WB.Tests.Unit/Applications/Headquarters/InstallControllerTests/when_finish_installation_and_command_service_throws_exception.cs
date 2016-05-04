using System;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Security;
using WB.Tests.Unit.Applications.Headquarters.InstallControllerTests;
using WB.UI.Headquarters.Controllers;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Headquarters.InterviewControllerTests
{
    internal class when_finish_installation_and_command_service_throws_exception : InstallControllerTestsContext
    {
        private Establish context = () =>
        {
            commandServiceMock.Setup(_ => _.Execute(Moq.It.IsAny<ICommand>(), Moq.It.IsAny<string>()))
                .Throws(new Exception());
            
            controller = CreateController(commandService: commandServiceMock.Object, logger: loggerMock.Object, authentication: authenticationServiceMock.Object);
        };

        Because of = () =>
            controller.Finish(model);

        It should_logger_call_fatal_method = () =>
            loggerMock.Verify(_ => _.Fatal(Moq.It.IsAny<string>(), Moq.It.IsAny<Exception>()));

        It should_user_never_be_signed_in_by_authentication_service = () =>
            authenticationServiceMock.Verify(_ => _.SignIn(Moq.It.IsAny<string>(), Moq.It.IsAny<bool>()), Times.Never);
        
        private static Mock<ICommandService> commandServiceMock = new Mock<ICommandService>();
        private static Mock<IFormsAuthentication> authenticationServiceMock = new Mock<IFormsAuthentication>();
        private static Mock<ILogger> loggerMock = new Mock<ILogger>();
        private static InstallController controller;

        private static UserModel model = new UserModel()
        {
            UserName = "hq",
            Password = "P@$$w0rd",
            ConfirmPassword = "P@$$w0rd",
            Email = "hq@wbcapi.org"
        };
    }
}
