using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using System.Web.Mvc;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Tests.Abc.TestFactories;
using WB.UI.Headquarters.Controllers;

namespace WB.Tests.Unit.Applications.Headquarters.ApiUserControllerTests
{
    internal class when_creating_api_user : ApiUserControllerTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context ()
        {
            inputModel = new UserModel
            {
                UserName = "apiTest",
                Password = "12345",
                ConfirmPassword = "12345"
            };
            userManagerMock = new Mock<TestHqUserManager>();
            userManagerMock.Setup(o => o.CreateUserAsync(Moq.It.IsAny<HqUser>(), inputModel.Password, UserRoles.ApiUser))
                .Returns(() => Task.FromResult(IdentityResult.Success));
            controller = CreateApiUserController(userManager: userManagerMock.Object);

            BecauseOf();
        }

        private void BecauseOf() => actionResult = controller.Create(inputModel).Result;

        [NUnit.Framework.Test] public void should_return_ViewResult () =>
            actionResult.Should().BeOfType<RedirectToRouteResult>();

        [NUnit.Framework.Test] public void should_user_be_created () =>
            userManagerMock.Verify(x => x.CreateUserAsync(Moq.It.IsAny<HqUser>(), Moq.It.IsAny<string>(), Moq.It.IsAny<UserRoles>()), Times.Once);

        private static Mock<TestHqUserManager> userManagerMock;
        private static ActionResult actionResult ;
        private static ApiUserController controller;
        private static UserModel inputModel;
    }


}
