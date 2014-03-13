using System.Linq;
using System.Security.Principal;
using System.Web.Mvc;
using Machine.Specifications;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using WB.Core.BoundedContexts.Headquarters.Authentication.Models;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Models;
using It = Machine.Specifications.It;

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

        It should_sign_in_provided_identity = () => authenticationManager.Verify(x => x.SignIn(Moq.It.IsAny<AuthenticationProperties>(), testIdentity));

        It should_sign_out_any_other_identity = () => authenticationManager.Verify(x => 
            x.SignOut(Moq.It.Is<string[]>(param => param.Contains(DefaultAuthenticationTypes.ExternalCookie))));
    }
}