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
    public class when_EditUser_action_invoked
    {
        Establish context = () =>
        {
            var userStore = Substitute.For<IUserStore<ApplicationUser>>();

            var userManager = Substitute.For<UserManager<ApplicationUser>>(userStore);
            applicationUser = new ApplicationUser("1")
            {
                IsAdministrator = true,
                IsHeadquarter = true
            };

            userManager.FindByIdAsync(null)
                .ReturnsForAnyArgs(Task.FromResult(applicationUser));

            controller = Create.UsersController(userManager);
        };

        Because of = async () => actionResult = await controller.EditAccount("1");

        It should_return_view = () => actionResult.ShouldBeOfExactType<ViewResult>();

        It should_fill_view_model = () =>
        {
            var model = actionResult.GetModel<AccountModel>();

            model.Id.ShouldEqual("1");
            model.AdminRoleEnabled.ShouldBeTrue();
            model.HeadquarterRoleEnabled.ShouldBeTrue();
        };

        private static UsersController controller;
        private static ActionResult actionResult;
        private static ApplicationUser applicationUser;
    }
}