using System.Web.Mvc;
using Machine.Specifications;
using NSubstitute;
using WB.UI.Headquarters.Controllers;
using It = Machine.Specifications.It;

namespace WB.UI.Headquarters.Tests.Controllers.AccountControllerSpecs
{
    [Subject(typeof(AccountController))]
    public class when_user_logges_in_with_return_url : user_credentials_are_valid
    {
        Establish context = () =>
        {
            controller.Url = Substitute.For<UrlHelper>();
            controller.Url.IsLocalUrl("/Action/Controller")
                .Returns(true);
        };

        Because of = async () => actionResult = await controller.Login(LoginModel, "/Action/Controller");

        It should_redirect_to_return_url = () =>
        {
            var redirectResult = actionResult as RedirectResult;
            redirectResult.ShouldNotBeNull();
            redirectResult.Url.ShouldEqual("/Action/Controller");
        };
    }
}