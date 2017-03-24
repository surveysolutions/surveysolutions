using System;
using Machine.Specifications;
using Moq;
using System.Web.Mvc;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Tests.Abc.TestFactories;
using WB.UI.Headquarters.Controllers;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Headquarters.ApiUserControllerTests
{
    internal class when_creating_api_user : ApiUserControllerTestContext
    {
        private Establish context = () =>
        {
            inputModel = new UserModel()
            {
                UserName = "apiTest",
                Password = "12345",
                ConfirmPassword = "12345"
            };
            controller = CreateApiUserController(userManager: userManagerMock.Object);
        };

        Because of = () =>
        {
            actionResult = controller.Create(inputModel).Result;
        };

        It should_return_ViewResult = () =>
            actionResult.ShouldBeOfExactType<RedirectToRouteResult>();

        It should_user_be_created = () =>
            userManagerMock.Verify(x => x.CreateUserAsync(Moq.It.IsAny<HqUser>(), Moq.It.IsAny<string>(), Moq.It.IsAny<UserRoles>()), Times.Once);

        private static Mock<TestHqUserManager> userManagerMock = new Mock<TestHqUserManager>();
        private static ActionResult actionResult ;
        private static ApiUserController controller;
        private static UserModel inputModel;
    }


}
