using System.Web.Mvc;
using Machine.Specifications;
using Microsoft.Owin.Security;
using Moq;
using WB.UI.Headquarters.Controllers;
using It = Machine.Specifications.It;

namespace WB.UI.Headquarters.Tests.Controllers.AccountControllerSpecs
{
    [Subject(typeof(AccountController))]
    public class when_user_signs_out
    {
        static Mock<IAuthenticationManager> authenticationManager;
        static AccountController controller;
        static ActionResult actionResult;

        Establish context = () =>
        {
            authenticationManager = new Mock<IAuthenticationManager>();
            controller = Create.AccountController(authenticationManager: authenticationManager.Object);
        };

        Because of = () => actionResult = controller.SignOut();

        It should_call_authentication_manager_to_logout = () => authenticationManager.Verify(x => x.SignOut());
        It should_redirect_to_login_page = () => {
            var redirectResult = actionResult as RedirectToRouteResult;
            redirectResult.ShouldNotBeNull();
            redirectResult.RouteValues["Action"].ShouldEqual("Login");
        };
    }
}