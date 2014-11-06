﻿using System;
using System.Web.Mvc;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Utility;
using Moq;
using Ncqrs.Commanding;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Tests.InstallControllerTests;
using It = Machine.Specifications.It;

namespace WB.UI.Headquarters.Tests.InterviewControllerTests
{
    internal class when_finish_installation_and_command_service_throws_exception : InstallControllerTestsContext
    {
        private Establish context = () =>
        {
            commandServiceMock.Setup(_ => _.Execute(Moq.It.IsAny<ICommand>(), Moq.It.IsAny<string>()))
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
