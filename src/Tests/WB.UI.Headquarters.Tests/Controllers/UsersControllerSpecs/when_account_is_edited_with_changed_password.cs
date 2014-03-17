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
    public class when_account_is_edited_with_changed_password
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
            applicationUser = new ApplicationUser("1")
            {
                UserName = model.UserName
            };

            userManager = Substitute.For<UserManager<ApplicationUser>>(Substitute.For<IUserStore<ApplicationUser>>());
            userManager.FindByIdAsync("1")
                .Returns(Task.FromResult(applicationUser));

            userManager.UpdateAsync(null)
                .ReturnsForAnyArgs(Task.FromResult(IdentityResult.Success));

            var passwordValidator = Substitute.For<IIdentityValidator<string>>();
            passwordValidator.ValidateAsync(model.Password)
                .Returns(Task.FromResult(IdentityResult.Success));
            userManager.PasswordValidator = passwordValidator;

            var passwordHasher = Substitute.For<IPasswordHasher>();
            passwordHasher.HashPassword(model.Password)
                .Returns("password1hash");
            userManager.PasswordHasher = passwordHasher;

            controller = Create.UsersController(userManager);
        };

        Because of = async () => actionResult = await controller.EditAccount("1", model);

        It should_change_user_password = () => applicationUser.PasswordHash.ShouldEqual("password1hash");

        It should_update_admin_role = () => applicationUser.IsAdministrator.ShouldBeTrue();

        It should_update_headquarter_role = () => applicationUser.IsHeadquarter.ShouldBeTrue();

        It should_redirect_to_index = () => actionResult.ShouldBeRedirectToAction("Index");

        It should_update_user_identity = () => userManager.Received().UpdateAsync(applicationUser);

        It should_add_updated_user_login_to_temp_data = () => controller.TempData["HighlightedUser"].ShouldEqual(applicationUser.UserName);

        private static UsersController controller;
        private static EditAccountModel model;
        private static ActionResult actionResult;
        private static ApplicationUser applicationUser;
        private static UserManager<ApplicationUser> userManager;
    }
}