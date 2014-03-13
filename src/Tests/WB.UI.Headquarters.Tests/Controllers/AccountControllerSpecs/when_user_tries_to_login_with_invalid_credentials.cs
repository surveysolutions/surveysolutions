using System.Security.Principal;
using System.Web.Mvc;
using Machine.Specifications;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Authentication.Models;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Models;
using WB.UI.Headquarters.Resources;
using It = Machine.Specifications.It;

namespace WB.UI.Headquarters.Tests.Controllers.AccountControllerSpecs
{
    [Subject(typeof(AccountController))]
    public class when_user_tries_to_login_with_invalid_credentials
    {
        static AccountController controller;
        static LoginModel loginModel;
        static ActionResult actionReult;

        Establish context = () =>
        {
            loginModel = new LoginModel
            {
                Login = "login",
                Password = "password"
            };

            var userStore = new Mock<IUserStore<ApplicationUser>>();
            var userManager = new Mock<UserManager<ApplicationUser>>(userStore.Object);

            controller = new AccountController(userManager.Object, Mock.Of<IAuthenticationManager>())
            {
                Url = Mock.Of<UrlHelper>()
            };
        };

        Because of = async () => actionReult = await controller.Login(loginModel, null);

        It should_add_model_error = () => controller.ModelState[""].Errors.ShouldContain(error => error.ErrorMessage == LoginPageResources.IvalidUserNameOrPassword);

        It should_return_view = () => actionReult.ShouldBeOfExactType<ViewResult>();
    }
}