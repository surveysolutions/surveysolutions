using System.Linq;
using System.Web.Mvc;
using Machine.Specifications;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using NSubstitute;
using WB.UI.Headquarters.Controllers;

namespace WB.UI.Headquarters.Tests.Controllers.AccountControllerSpecs
{
    [Subject(typeof(AccountController))]
    public class when_user_logges_in_without_return_url: user_credentials_are_valid
    {
        Because of = async () => actionResult = await controller.Login(LoginModel, null);

        It should_redirect_to_index = () =>
        {
            var redirectToRouteResult = actionResult as RedirectToRouteResult;
            redirectToRouteResult.ShouldNotBeNull();

            redirectToRouteResult.RouteValues["controller"].ShouldEqual("Home");
            redirectToRouteResult.RouteValues["action"].ShouldEqual("Index");
        };

        It should_sign_in_provided_identity = () => authenticationManager.Received().SignIn(Arg.Any<AuthenticationProperties>(), testIdentity);

        It should_sign_out_any_other_identity = () => authenticationManager.Received()
            .SignOut(Arg.Is<string[]>(param => param.Contains(DefaultAuthenticationTypes.ExternalCookie)));
    }
}