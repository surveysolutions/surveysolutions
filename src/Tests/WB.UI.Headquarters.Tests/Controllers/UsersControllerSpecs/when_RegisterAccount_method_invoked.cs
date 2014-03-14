using System.Web.Mvc;
using Machine.Specifications;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Models;

namespace WB.UI.Headquarters.Tests.Controllers.UsersControllerSpecs
{
    [Subject(typeof (UsersController))]
    public class when_RegisterAccount_method_invoked
    {
        Establish context = () =>
        {
            controller = Create.UsersController();
        };

        Because of = () => actionResult = controller.RegisterAccount();

        It should_allow_change_user_name = () =>
        {
            var viewResult = actionResult as ViewResult;
            viewResult.ShouldNotBeNull();
            var accountModel = viewResult.Model as AccountModel;
            accountModel.ShouldNotBeNull();
            accountModel.UserNameChangeAllowed.ShouldBeTrue();
        };

        static UsersController controller;
        static ActionResult actionResult;
    }
}