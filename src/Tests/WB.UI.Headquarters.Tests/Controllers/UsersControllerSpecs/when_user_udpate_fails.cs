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
    public class when_user_udpate_fails
    {
        Establish context = () =>
        {
            model = new EditAccountModel
            {
                UserName = "user",
                Password = "password1",
                AdminRoleEnabled = true,
                HeadquarterRoleEnabled = true,
                Id = "1"
            };
            applicationUser = new ApplicationUser("1");

            userManager = Substitute.For<UserManager<ApplicationUser>>(Substitute.For<IUserStore<ApplicationUser>>());
            userManager.FindByNameAsync("1")
                .Returns(Task.FromResult(applicationUser));

            userManager.UpdateAsync(null)
                .ReturnsForAnyArgs(Task.FromResult(IdentityResult.Failed("error")));

            var passwordValidator = Substitute.For<IIdentityValidator<string>>();
            passwordValidator.ValidateAsync(model.Password)
                .Returns(Task.FromResult(IdentityResult.Success));
            userManager.PasswordValidator = passwordValidator;

            userManager.PasswordHasher = Substitute.For<IPasswordHasher>();

            controller = Create.UsersController(userManager);
        };

        Because of = async () => actionResult = await controller.EditAccount(model.UserName, model);

        It should_return_view = () => actionResult.ShouldBeOfExactType<ViewResult>();

        It should_add_model_error = () => controller.ModelState[""].Errors.ShouldContain(x => x.ErrorMessage == "error");

        private static UsersController controller;
        private static EditAccountModel model;
        private static ActionResult actionResult;
        private static ApplicationUser applicationUser;
        private static UserManager<ApplicationUser> userManager;
    }
}