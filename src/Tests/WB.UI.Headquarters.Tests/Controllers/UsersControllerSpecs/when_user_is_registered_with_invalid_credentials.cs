using System.Linq;
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
    public class when_user_is_registered_with_invalid_credentials
    {
        Establish context = () =>
        {
            var userManager = Substitute.For<UserManager<ApplicationUser>>(Substitute.For<IUserStore<ApplicationUser>>());
            userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
                       .Returns(Task.FromResult(IdentityResult.Failed("error")));

            controller = Create.UsersController(userManager);
            model = new RegisterAccountModel();
        };

        Because of = async () => actionResult = await controller.RegisterAccount(model);

        It should_add_model_error = () =>
        {
            ModelState modelState = controller.ModelState[""];
            modelState.ShouldNotBeNull();
            modelState.Errors.Count.ShouldEqual(1);
            modelState.Errors.First().ErrorMessage.ShouldEqual("error");

        };

        It should_return_view = () => actionResult.ShouldBeOfExactType<ViewResult>();

        private static UsersController controller;
        private static RegisterAccountModel model;
        private static ActionResult actionResult;
    }
}