using System.Web.Mvc;
using Machine.Specifications;
using Microsoft.Owin.Security;
using NSubstitute;
using WB.UI.Headquarters.Controllers;

namespace WB.UI.Headquarters.Tests.Controllers.AccountControllerSpecs
{
    [Subject(typeof(AccountController))]
    public class when_user_signs_out
    {
        static IAuthenticationManager authenticationManager;
        static AccountController controller;
        static ActionResult actionResult;

        Establish context = () =>
        {
            authenticationManager = Substitute.For<IAuthenticationManager>();
            controller = Create.AccountController(authenticationManager: authenticationManager);
        };

        Because of = () => actionResult = controller.SignOut();

        It should_call_authentication_manager_to_logout = () => authenticationManager.Received().SignOut();

        It should_redirect_to_login_page = () => {
            var redirectResult = actionResult as RedirectToRouteResult;
            redirectResult.ShouldNotBeNull();
            redirectResult.RouteValues["Action"].ShouldEqual("Login");
        };
    }
}