using System.Linq;
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
    public class when_user_is_registered_with_invalid_credentials
    {
        Establish context = () =>
        {
            var userManager = new Mock<UserManager<ApplicationUser>>(Mock.Of<IUserStore<ApplicationUser>>());
            userManager.Setup(x => x.CreateAsync(Moq.It.IsAny<ApplicationUser>(), Moq.It.IsAny<string>()))
                       .ReturnsAsync(IdentityResult.Failed("error"));

            controller = Create.UsersController(userManager.Object);
            model = new AccountModel();
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
        private static AccountModel model;
        private static ActionResult actionResult;
    }
}