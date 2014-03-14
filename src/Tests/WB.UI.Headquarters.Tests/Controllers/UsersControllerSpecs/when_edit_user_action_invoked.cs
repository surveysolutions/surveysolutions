using System.Threading.Tasks;
using System.Web.Mvc;
using Machine.Specifications;
using Microsoft.AspNet.Identity;
using NSubstitute;
using WB.Core.BoundedContexts.Headquarters.Authentication.Models;
using WB.UI.Headquarters.Controllers;

namespace WB.UI.Headquarters.Tests.Controllers.UsersControllerSpecs
{
    [Subject(typeof(UsersController))]
    public class when_EditUser_action_invoked
    {
        Establish context = () =>
        {
            var userStore = Substitute.For<IUserStore<ApplicationUser>>();

            var userManager = Substitute.For<UserManager<ApplicationUser>>(userStore);
            userManager.FindAsync(null, null)
                .ReturnsForAnyArgs(Task.FromResult(new ApplicationUser("1")));

            controller = Create.UsersController();
        };

        Because of = async () => actionResult = await controller.EditAccount("1");

        //It should_return_view = () => actionResult.ShouldBeOfExactType<ViewResult>();

        private static UsersController controller;
        private static ActionResult actionResult;
    }
}