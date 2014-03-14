using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Mvc;
using Machine.Specifications;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using NSubstitute;
using WB.Core.BoundedContexts.Headquarters.Authentication.Models;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Models;

namespace WB.UI.Headquarters.Tests.Controllers.AccountControllerSpecs
{
    public class user_credentials_are_valid
    {
        protected static ActionResult actionResult;
        protected static AccountController controller;
        protected static LoginModel LoginModel;
        protected static IAuthenticationManager authenticationManager;
        protected static ClaimsIdentity testIdentity;

        Establish context = () =>
        {
            LoginModel = new LoginModel
            {
                Login = "login",
                Password = "password"
            };

            var userStore = Substitute.For<IUserStore<ApplicationUser>>();

            var userManager = Substitute.For<UserManager<ApplicationUser>>(userStore);
            userManager.FindAsync(LoginModel.Login, LoginModel.Password)
                .Returns(Task.FromResult(new ApplicationUser("1")));

            testIdentity = new GenericIdentity("test identity");

            userManager.CreateIdentityAsync(Arg.Any<ApplicationUser>(), DefaultAuthenticationTypes.ApplicationCookie)
                .Returns(Task.FromResult(testIdentity));

            authenticationManager = Substitute.For<IAuthenticationManager>();
            controller = Create.AccountController(userManager, authenticationManager);
            controller.Url = Substitute.For<UrlHelper>();
        };
    }
}