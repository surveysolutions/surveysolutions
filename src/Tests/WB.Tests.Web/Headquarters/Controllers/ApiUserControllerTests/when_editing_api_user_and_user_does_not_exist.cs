using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using System.Web.Mvc;
using FluentAssertions;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Tests.Abc.TestFactories;
using WB.UI.Headquarters.Controllers;

namespace WB.Tests.Unit.Applications.Headquarters.ApiUserControllerTests
{
    internal class when_editing_api_user_and_user_does_not_exist : ApiUserControllerTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context ()
        {
            inputModel = new UserEditModel()
            {
                UserName = "apiTest",
                Password = "12345",
                ConfirmPassword = "12345"
            };

            userManagerMock.Setup(x => x.FindByIdAsync(Moq.It.IsAny<Guid>())).Returns(Task.FromResult<HqUser>(null));

            controller = CreateApiUserController(userManager: userManagerMock.Object);
            BecauseOf();
        }

        private void BecauseOf() => actionResult = controller.Edit(inputModel).Result;

        [NUnit.Framework.Test] public void should_return_ViewResult () =>
            actionResult.Should().BeOfType<ViewResult>();

        [NUnit.Framework.Test] public void should_execute_CreateUserCommand_onece () =>
            controller.ModelState.SelectMany(x=>x.Value.Errors).Select(x=>x.ErrorMessage).Should().Contain("Could not update user information because current user does not exist");


        private static Mock<TestHqUserManager> userManagerMock = new Mock<TestHqUserManager>();
        private static ActionResult actionResult ;
        private static ApiUserController controller;
        private static UserEditModel inputModel;
    }


}
