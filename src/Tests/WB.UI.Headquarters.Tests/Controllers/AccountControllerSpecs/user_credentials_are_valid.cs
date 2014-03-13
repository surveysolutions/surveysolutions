using System.Linq;
using System.Security.Principal;
using System.Web.Mvc;
using Machine.Specifications;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Authentication.Models;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Models;
using It = Machine.Specifications.It;

namespace WB.UI.Headquarters.Tests.Controllers.AccountControllerSpecs
{
    public class user_credentials_are_valid
    {
        protected static ActionResult actionResult;
        protected static AccountController controller;
        protected static LoginModel LoginModel;
        protected static Mock<IAuthenticationManager> authenticationManager;
        protected static GenericIdentity testIdentity;

        Establish context = () =>
        {
            LoginModel = new LoginModel
            {
                Login = "login",
                Password = "password"
            };

            var userStore = new Mock<IUserStore<ApplicationUser>>();

            var userManager = new Mock<UserManager<ApplicationUser>>(userStore.Object);
            userManager.Setup(x => x.FindAsync(LoginModel.Login, LoginModel.Password))
                .ReturnsAsync(new ApplicationUser("1"));
            testIdentity = new GenericIdentity("test identity");
            userManager.Setup(x => x.CreateIdentityAsync(Moq.It.IsAny<ApplicationUser>(), DefaultAuthenticationTypes.ApplicationCookie))
                .ReturnsAsync(testIdentity);

            authenticationManager = new Mock<IAuthenticationManager>();
            controller = Create.AccountController(userManager.Object, authenticationManager.Object);
            controller.Url = Mock.Of<UrlHelper>();
        };
    }
}