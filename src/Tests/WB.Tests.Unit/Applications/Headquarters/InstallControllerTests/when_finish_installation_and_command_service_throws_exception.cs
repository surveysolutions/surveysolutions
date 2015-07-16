using System;
using System.Web.Mvc;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Commanding;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Tests.Unit.Applications.Headquarters.InstallControllerTests;
using WB.UI.Headquarters.Controllers;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Headquarters.InterviewControllerTests
{
    internal class when_finish_installation_and_command_service_throws_exception : InstallControllerTestsContext
    {
        private Establish context = () =>
        {
            commandServiceMock.Setup(_ => _.Execute(Moq.It.IsAny<ICommand>(), Moq.It.IsAny<string>(), Moq.It.IsAny<bool>()))
                .Throws(new Exception());
            
            controller = CreateController(commandService: commandServiceMock.Object, logger: loggerMock.Object);
        };

        Because of = () =>
            controller.Finish(model);

        It should_logger_call_fatal_method = () =>
            loggerMock.Verify(_ => _.Fatal(Moq.It.IsAny<string>(), Moq.It.IsAny<Exception>()));
        
        private static Mock<ICommandService> commandServiceMock = new Mock<ICommandService>();
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
