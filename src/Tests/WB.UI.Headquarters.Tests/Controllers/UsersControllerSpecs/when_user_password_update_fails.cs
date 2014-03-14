using System.Threading.Tasks;
using System.Web.Mvc;
using Machine.Specifications;
using Microsoft.AspNet.Identity;
using NSubstitute;
using WB.Core.BoundedContexts.Headquarters.Authentication.Models;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Models;

namespace WB.UI.Headquarters.Tests.Controllers.UsersControllerSpecs
{
    [Subject(typeof(UsersController))]
    public class when_user_password_update_fails
    {
        Establish context = () =>
        {
            model = new AccountModel
            {
                UserName = "user",
                Password = "password1",
                AdminRoleEnabled = true,
                HeadquarterRoleEnabled = true,
                Id = "1"
            };
            applicationUser = new ApplicationUser("1"){PasswordHash = "hash"};

            userManager = Substitute.For<UserManager<ApplicationUser>>(Substitute.For<IUserStore<ApplicationUser>>());
            userManager.FindByIdAsync("1")
                .Returns(Task.FromResult(applicationUser));

            var passwordValidator = Substitute.For<IIdentityValidator<string>>();
            passwordValidator.ValidateAsync(model.Password)
                .Returns(Task.FromResult(IdentityResult.Failed("error")));
            userManager.PasswordValidator = passwordValidator;

            controller = Create.UsersController(userManager);
        };

        Because of = async () => actionResult = await controller.EditAccount("1", model);

        It should_not_update_user_identity = () => userManager.DidNotReceiveWithAnyArgs().UpdateAsync(null);

        It should_add_model_error = () => controller.ModelState[""].Errors.ShouldContain(x => x.ErrorMessage == "error");

        It should_return_view = () => actionResult.ShouldBeOfExactType<ViewResult>();

        private static UsersController controller;
        private static AccountModel model;
        private static ActionResult actionResult;
        private static ApplicationUser applicationUser;
        private static UserManager<ApplicationUser> userManager;
    }
}