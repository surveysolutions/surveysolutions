using System;
using System.Linq;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Controllers;
using WB.UI.Shared.Web.Extensions;
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

            identityManagerMock.Setup(x => x.GetUserByIdAsync(Moq.It.IsAny<Guid>())).Returns(Task.FromResult<HqUser>(null));

            controller = CreateApiUserController(identityManager: identityManagerMock.Object);
        };

        Because of = () =>
        {
            actionResult = controller.Edit(inputModel).Result;
        };

        It should_return_ViewResult = () =>
            actionResult.ShouldBeOfExactType<ViewResult>();

        It should_execute_CreateUserCommand_onece = () =>
            controller.ModelState.SelectMany(x=>x.Value.Errors).Select(x=>x.ErrorMessage).ShouldContain("Could not update user information because current user does not exist");


        private static Mock<IIdentityManager> identityManagerMock = new Mock<IIdentityManager>();
        private static ActionResult actionResult ;
        private static ApiUserController controller;
        private static UserEditModel inputModel;
    }


}
