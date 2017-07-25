using System.Net;
using System.Net.Http;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.CommandBus;
using WB.UI.Designer.Api;
using WB.UI.Designer.Code;
using WB.UI.Designer.Code.Implementation;


namespace WB.Tests.Unit.Designer.Applications.CommandApiControllerTests
{
    internal class when_user_has_no_access_rights : CommandApiControllerTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            
            model = new CommandController.CommandExecutionModel();
            commandInflaterMock = new Mock<ICommandInflater>();
            commandInflaterMock.Setup(x => x.PrepareDeserializedCommandForExecution(Moq.It.IsAny<ICommand>()))
                .Throws(new CommandInflaitingException(CommandInflatingExceptionType.Forbidden, "test"));

            controller = CreateCommandController(commandInflater:commandInflaterMock.Object);
            BecauseOf();
        }

        private void BecauseOf() =>
            message = controller.Post(model);

        [NUnit.Framework.Test] public void should_not_be_null () =>
            message.ShouldNotBeNull();

        [NUnit.Framework.Test] public void should_not_be_correct_status () =>
            message.StatusCode.ShouldEqual(HttpStatusCode.Forbidden);

        private static CommandController controller;
        private static CommandController.CommandExecutionModel model;
        private static Mock<ICommandInflater> commandInflaterMock;
        private static HttpResponseMessage message;
    }
}
