using Machine.Specifications;
using Moq;
using System.Web.Mvc;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Controllers;
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
            controller = CreateApiUserController(identityManager: identityManagerMock.Object);
        };

        Because of = () =>
        {
            actionResult = controller.Create(inputModel).Result;
        };

        It should_return_ViewResult = () =>
            actionResult.ShouldBeOfExactType<RedirectToRouteResult>();

        It should_execute_CreateUserCommand_onece = () =>
            identityManagerMock.Verify(x => x.CreateUserAsync(Moq.It.IsAny<ApplicationUser>(), Moq.It.IsAny<string>(), Moq.It.IsAny<UserRoles>()), Times.Once);


        private static Mock<IIdentityManager> identityManagerMock = new Mock<IIdentityManager>();
        private static ActionResult actionResult ;
        private static ApiUserController controller;
        private static UserModel inputModel;
    }


}
