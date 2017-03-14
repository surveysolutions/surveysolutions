using System;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Tests.Abc;
using WB.UI.Headquarters.Controllers;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Headquarters.ApiUserControllerTests
{
    internal class when_editing_api_user : ApiUserControllerTestContext
    {
        private Establish context = () =>
        {
            var userId = Guid.NewGuid();

            inputModel = new UserEditModel()
            {
                UserName = "apiTest",
                Password = "12345",
                ConfirmPassword = "12345",
                Id = userId
            };

            identityManagerMock.Setup(x => x.GetUserByIdAsync(userId)).Returns(Task.FromResult(Create.Entity.ApplicationUser()));
            identityManagerMock.Setup(x => x.UpdateUserAsync(Moq.It.IsAny<HqUser>(), Moq.It.IsAny<string>())).Returns(Task.FromResult(IdentityResult.Success));

            controller = CreateApiUserController(identityManager: identityManagerMock.Object);
        };

        Because of = () => actionResult = controller.Edit(inputModel).Result;

        It should_return_ViewResult = () =>
            actionResult.ShouldBeOfExactType<RedirectToRouteResult>();

        It should_execute_CreateUserCommand_onece = () =>
            identityManagerMock.Verify(x => x.UpdateUserAsync(Moq.It.IsAny<HqUser>(), Moq.It.IsAny<string>()), Times.Once);

        private static Mock<IIdentityManager> identityManagerMock = new Mock<IIdentityManager>();
        private static ActionResult actionResult ;
        private static ApiUserController controller;
        private static UserEditModel inputModel;
    }


}
