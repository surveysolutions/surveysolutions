using System;
using Moq;
using System.Web.Mvc;
using FluentAssertions;
using Microsoft.AspNet.Identity;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Tests.Abc;
using WB.Tests.Abc.TestFactories;
using WB.UI.Headquarters.Controllers;

namespace WB.Tests.Unit.Applications.Headquarters.ApiUserControllerTests
{
    internal class when_editing_api_user : ApiUserControllerTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
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

            BecauseOf();
        }

        private void BecauseOf() => actionResult = controller.Edit(inputModel).Result;

        [NUnit.Framework.Test] public void should_return_ViewResult () =>
            actionResult.Should().BeOfType<RedirectToRouteResult>();

        [NUnit.Framework.Test] public void should_execute_CreateUserCommand_onece () =>
            identityManagerMock.Verify(x => x.UpdateAsync(Moq.It.IsAny<HqUser>()), Times.Once);

        private static Mock<TestHqUserManager> identityManagerMock = new Mock<TestHqUserManager>();
        private static ActionResult actionResult ;
        private static ApiUserController controller;
        private static UserEditModel inputModel;
    }
}
