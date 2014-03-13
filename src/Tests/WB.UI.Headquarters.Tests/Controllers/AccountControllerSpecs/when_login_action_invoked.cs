using System.Web.Mvc;
using Machine.Specifications;
using WB.UI.Headquarters.Controllers;

namespace WB.UI.Headquarters.Tests.Controllers.AccountControllerSpecs
{
    public class when_login_action_invoked
    {
        static AccountController controller;
        static ActionResult actionResult;

        Establish context = () =>
        {
            controller = Create.AccountController();
        };

        Because of = () => actionResult = controller.Login();

        It should_return_view = () => actionResult.ShouldBeOfExactType<ViewResult>();
    }
}