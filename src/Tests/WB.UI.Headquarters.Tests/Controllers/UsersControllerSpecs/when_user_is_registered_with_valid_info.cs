using System.Threading.Tasks;
using System.Web.Mvc;
using Machine.Specifications;
using Microsoft.AspNet.Identity;
using NSubstitute;
using WB.Core.BoundedContexts.Headquarters.Authentication.Models;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Models;

namespace WB.UI.Headquarters.Tests.Controllers.UsersControllerSpecs
{
    [Subject(typeof(UsersController))]
    public class when_user_is_registered_with_valid_info
    {
        Establish context = () =>
        {
            userManager = Substitute.For<UserManager<ApplicationUser>>(Substitute.For<IUserStore<ApplicationUser>>());
            userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
                       .Returns(Task.FromResult(IdentityResult.Success));
            controller = Create.UsersController(userManager);

            accountModel = new RegisterAccountModel()
            {
                UserName = "user",
                Password = "password",
                AdminRoleEnabled = true,
                HeadquarterRoleEnabled = true
            };
        };

        Because of = async () => actionResult = await controller.RegisterAccount(accountModel);

        It should_create_user_identity = () => userManager.Received().CreateAsync(Arg.Any<ApplicationUser>(), accountModel.Password);

        It should_redirect_to_index_action = () =>
        {
            var result = actionResult as RedirectToRouteResult;
            result.ShouldNotBeNull();
            result.RouteValues["action"].ShouldEqual("Index");
        };

        It should_put_highlighted_user_login_to_temp_data = () => controller.TempData["HighlightedUser"].ShouldEqual(accountModel.UserName);

        static UsersController controller;
        static RegisterAccountModel accountModel;
        static ActionResult actionResult;
        static UserManager<ApplicationUser> userManager;
    }
}