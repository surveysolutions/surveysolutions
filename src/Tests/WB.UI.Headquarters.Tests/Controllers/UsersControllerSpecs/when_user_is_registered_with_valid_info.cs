using System.Web.Mvc;
using Machine.Specifications;
using Microsoft.AspNet.Identity;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Authentication.Models;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Models;
using It = Machine.Specifications.It;

namespace WB.UI.Headquarters.Tests.Controllers.UsersControllerSpecs
{
    [Subject(typeof(UsersController))]
    public class when_user_is_registered_with_valid_info
    {
        Establish context = () =>
        {
            userManager = new Mock<UserManager<ApplicationUser>>(Mock.Of<IUserStore<ApplicationUser>>());
            userManager.Setup(x => x.CreateAsync(Moq.It.IsAny<ApplicationUser>(), Moq.It.IsAny<string>()))
                       .ReturnsAsync(IdentityResult.Success);
            controller = Create.UsersController(userManager.Object);

            accountModel = new AccountModel
            {
                UserName = "user",
                Password = "password",
                AdminRoleEnabled = true,
                HeadquarterRoleEnabled = true
            };
        };

        Because of = async () => actionResult = await controller.RegisterAccount(accountModel);

        It should_create_user_identity = () => userManager.Verify(x => x.CreateAsync(Moq.It.IsAny<ApplicationUser>(), accountModel.Password));

        It should_redirect_to_index_action = () =>
        {
            var result = actionResult as RedirectToRouteResult;
            result.ShouldNotBeNull();
            result.RouteValues["action"].ShouldEqual("Index");
        };

        It should_put_highlighted_user_login_to_temp_data = () => controller.TempData["HighlightedUser"].ShouldEqual(accountModel.UserName);

        static UsersController controller;
        static AccountModel accountModel;
        static ActionResult actionResult;
        static Mock<UserManager<ApplicationUser>> userManager;
    }
}