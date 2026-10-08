using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.UI.Designer.Areas.Identity.Pages.Account;
using WB.UI.Designer.Areas.Identity.Pages.Account.Manage;

namespace WB.Tests.Unit.Designer.Areas.Identity.Pages.Account
{
    // A legacy account keeps its SHA1 hash valid only while PasswordSalt is present, so committing a cleared salt
    // from a failed password operation locks the account out permanently.
    [TestFixture]
    public class PasswordPagesTransactionTests
    {
        private const string LegacySalt = "legacy-salt";

        [Test]
        public async Task ChangePassword_when_change_fails_marks_rollback_and_keeps_legacy_salt()
        {
            var user = new DesignerIdentityUser { PasswordSalt = LegacySalt };
            var userManager = CreateUserManager();
            userManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            userManager.Setup(m => m.ChangePasswordAsync(user, It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Incorrect password." }));

            var rollbackState = new TransactionRollbackState();
            var page = CreateChangePasswordPage(userManager, rollbackState);

            await page.OnPostAsync();

            Assert.That(rollbackState.IsRollbackOnly, Is.True);
            Assert.That(user.PasswordSalt, Is.EqualTo(LegacySalt));
        }

        [Test]
        public async Task ChangePassword_when_change_succeeds_does_not_mark_rollback()
        {
            var user = new DesignerIdentityUser();
            var userManager = CreateUserManager();
            userManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            userManager.Setup(m => m.ChangePasswordAsync(user, It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            var rollbackState = new TransactionRollbackState();
            var page = CreateChangePasswordPage(userManager, rollbackState);

            await page.OnPostAsync();

            Assert.That(rollbackState.IsRollbackOnly, Is.False);
        }

        [Test]
        public async Task ResetPassword_when_reset_fails_marks_rollback_and_keeps_legacy_salt()
        {
            var user = new DesignerIdentityUser { PasswordSalt = LegacySalt };
            var userManager = CreateUserManager();
            userManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
            userManager.Setup(m => m.ResetPasswordAsync(user, It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Invalid token." }));

            var rollbackState = new TransactionRollbackState();
            var page = CreateResetPasswordPage(userManager, rollbackState);

            await page.OnPostAsync();

            Assert.That(rollbackState.IsRollbackOnly, Is.True);
            Assert.That(user.PasswordSalt, Is.EqualTo(LegacySalt));
        }

        [Test]
        public async Task ResetPassword_when_reset_succeeds_clears_legacy_salt_without_rollback()
        {
            var user = new DesignerIdentityUser { PasswordSalt = LegacySalt };
            var userManager = CreateUserManager();
            userManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
            userManager.Setup(m => m.ResetPasswordAsync(user, It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            var rollbackState = new TransactionRollbackState();
            var page = CreateResetPasswordPage(userManager, rollbackState);

            await page.OnPostAsync();

            Assert.That(rollbackState.IsRollbackOnly, Is.False);
            Assert.That(user.PasswordSalt, Is.Null);
        }

        [Test]
        public async Task ConfirmEmail_when_confirmation_fails_marks_rollback()
        {
            var user = new DesignerIdentityUser();
            var userManager = CreateUserManager();
            userManager.Setup(m => m.FindByIdAsync("user")).ReturnsAsync(user);
            userManager.Setup(m => m.ConfirmEmailAsync(user, "code"))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Email is already in use." }));

            var rollbackState = new TransactionRollbackState();
            var page = new ConfirmEmailModel(userManager.Object, rollbackState)
            {
                UserId = "user",
                Code = "code",
                PageContext = new PageContext { HttpContext = new DefaultHttpContext() }
            };
            page.TempData = new TempDataDictionary(page.PageContext.HttpContext, Mock.Of<ITempDataProvider>());

            await page.OnPostAsync();

            Assert.That(rollbackState.IsRollbackOnly, Is.True);
        }

        private static ChangePasswordModel CreateChangePasswordPage(Mock<UserManager<DesignerIdentityUser>> userManager,
            ITransactionRollbackState rollbackState)
        {
            var signInManager = CreateSignInManager(userManager.Object);
            signInManager.Setup(m => m.RefreshSignInAsync(It.IsAny<DesignerIdentityUser>())).Returns(Task.CompletedTask);

            return new ChangePasswordModel(userManager.Object, signInManager.Object, rollbackState)
            {
                Input = new ChangePasswordModel.InputModel
                {
                    OldPassword = "old",
                    NewPassword = "new",
                    ConfirmPassword = "new"
                },
                PageContext = new PageContext { HttpContext = new DefaultHttpContext() }
            };
        }

        private static ResetPasswordModel CreateResetPasswordPage(Mock<UserManager<DesignerIdentityUser>> userManager,
            ITransactionRollbackState rollbackState)
            => new ResetPasswordModel(userManager.Object, rollbackState)
            {
                Input = new ResetPasswordModel.InputModel
                {
                    UserId = "user",
                    Code = "code",
                    Password = "new",
                    ConfirmPassword = "new"
                },
                PageContext = new PageContext { HttpContext = new DefaultHttpContext() }
            };

        private static Mock<UserManager<DesignerIdentityUser>> CreateUserManager()
        {
            var store = new Mock<IUserStore<DesignerIdentityUser>>();
            return new Mock<UserManager<DesignerIdentityUser>>(store.Object, null, null, null, null, null, null, null, null);
        }

        private static Mock<SignInManager<DesignerIdentityUser>> CreateSignInManager(UserManager<DesignerIdentityUser> userManager)
            => new Mock<SignInManager<DesignerIdentityUser>>(
                userManager,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<DesignerIdentityUser>>(),
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<ILogger<SignInManager<DesignerIdentityUser>>>(),
                Mock.Of<IAuthenticationSchemeProvider>(),
                Mock.Of<IUserConfirmation<DesignerIdentityUser>>());
    }
}
