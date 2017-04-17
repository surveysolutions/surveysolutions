﻿using System;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Tests.Abc;
using WB.Tests.Abc.TestFactories;
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

            identityManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(Create.Entity.HqUser());
            identityManagerMock.Setup(x => x.UpdateAsync(Moq.It.IsAny<HqUser>())).ReturnsAsync(IdentityResult.Success);

            controller = CreateApiUserController(userManager: identityManagerMock.Object);
        };

        Because of = () => actionResult = controller.Edit(inputModel).Result;

        It should_return_ViewResult = () =>
            actionResult.ShouldBeOfExactType<RedirectToRouteResult>();

        It should_execute_CreateUserCommand_onece = () =>
            identityManagerMock.Verify(x => x.UpdateAsync(Moq.It.IsAny<HqUser>()), Times.Once);

        private static Mock<TestHqUserManager> identityManagerMock = new Mock<TestHqUserManager>();
        private static ActionResult actionResult ;
        private static ApiUserController controller;
        private static UserEditModel inputModel;
    }
}
