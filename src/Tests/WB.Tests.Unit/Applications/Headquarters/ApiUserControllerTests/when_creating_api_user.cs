using Machine.Specifications;
using Moq;
using System.Web.Mvc;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Controllers;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Headquarters.ApiUserControllerTests
{
    internal class when_creating_api_user : ApiUserControllerTestContext
    {
        Establish context = () =>
        {
            inputModel = new UserModel()
            {
                UserName = "apiTest",
                Password = "12345",
                ConfirmPassword = "12345"
            };
            controller = CreateApiUserController(commandServiceMock.Object);
        };

        Because of = () =>
        {
            actionResult = controller.Create(inputModel);
        };

        It should_return_ViewResult = () =>
            actionResult.ShouldBeOfExactType<RedirectToRouteResult>();

        It should_execute_CreateUserCommand_onece = () =>
            commandServiceMock.Verify(x => x.Execute(Moq.It.IsAny<CreateUserCommand>(), Moq.It.IsAny<string>()),Times.Once);


        private static Mock<ICommandService> commandServiceMock = new Mock<ICommandService>();
        private static ActionResult actionResult ;
        private static ApiUserController controller;
        private static UserModel inputModel;
    }


}
