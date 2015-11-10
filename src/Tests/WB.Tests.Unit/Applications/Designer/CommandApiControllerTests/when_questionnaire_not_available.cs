using System;
using System.Web.Http;
using Moq;
using Machine.Specifications;
using WB.UI.Designer.Api;
using WB.UI.Designer.Code;
using It = Machine.Specifications.It;
using WB.Core.Infrastructure.CommandBus;
using WB.UI.Designer.Code.Implementation;
using System.Net;
using System.Net.Http;

namespace WB.Tests.Unit.Applications.Designer.CommandApiControllerTests
{
    internal class when_questionnaire_not_available : CommandApiControllerTestContext
    {
        Establish context = () =>
        {
            
            model = new CommandController.CommandExecutionModel();
            commandInflaterMock = new Mock<ICommandInflater>();
            commandInflaterMock.Setup(x => x.PrepareDeserializedCommandForExecution(Moq.It.IsAny<ICommand>()))
                .Throws(new CommandInflaitingException(CommandInflatingExceptionType.EntityNotFound, "test"));

            controller = CreateCommandController(commandInflater:commandInflaterMock.Object);
        };

        Because of = () =>
            message = controller.Post(model);

        It should_not_be_null = () =>
            message.ShouldNotBeNull();

        It should_not_be_correct_status = () =>
            message.StatusCode.ShouldEqual(HttpStatusCode.NotFound);

        private static CommandController controller;
        private static CommandController.CommandExecutionModel model;
        private static Mock<ICommandInflater> commandInflaterMock;
        private static HttpResponseMessage message;
    }
}
