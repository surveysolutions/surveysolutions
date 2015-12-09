using Machine.Specifications;
using Moq;
using System.Web.Mvc;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Controllers;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.UI.Designer.BootstrapSupport;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Headquarters.ApiUserControllerTests
{
    internal class when_editing_api_user_and_user_does_not_exist : ApiUserControllerTestContext
    {
        Establish context = () =>
        {
            inputModel = new UserEditModel()
            {
                UserName = "apiTest",
                Password = "12345",
                ConfirmPassword = "12345"
            };
            controller = CreateApiUserController(commandServiceMock.Object);
        };

        Because of = () =>
        {
            actionResult = controller.Edit(inputModel);
        };

        It should_return_ViewResult = () =>
            actionResult.ShouldBeOfExactType<ViewResult>();

        It should_execute_CreateUserCommand_onece = () =>
            controller.TempData[Alerts.ERROR].ShouldEqual("Could not update user information because current user does not exist");


        private static Mock<ICommandService> commandServiceMock = new Mock<ICommandService>();
        private static ActionResult actionResult ;
        private static ApiUserController controller;
        private static UserEditModel inputModel;
    }


}
