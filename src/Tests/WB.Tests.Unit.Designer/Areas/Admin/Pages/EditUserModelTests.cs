using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.UI.Designer.Areas.Admin.Pages;

namespace WB.Tests.Unit.Designer.Areas.Admin.Pages
{
    // Identity reports validation failures as IdentityResult while the user stays modified in the change tracker,
    // so every failure must veto the request transaction or TransactionFilter would commit the rejected values.
    [TestFixture]
    [TestOf(typeof(EditUserModel))]
    public class EditUserModelTests
    {
        private const string Email = "user@example.org";
        private static readonly IdentityResult Rejected =
            IdentityResult.Failed(new IdentityError { Description = "User name is invalid." });

        [Test]
        public async Task when_update_is_rejected_marks_rollback_and_reports_error()
        {
            var user = NewUser();
            var userManager = CreateUserManager(user);
            userManager.Setup(m => m.UpdateAsync(user)).ReturnsAsync(Rejected);
            var rollbackState = new TransactionRollbackState();
            var page = CreatePage(userManager, rollbackState);

            await page.OnPostAsync(user.Id.ToString());

            Assert.That(rollbackState.IsRollbackOnly, Is.True);
            Assert.That(page.ErrorMessage, Is.EqualTo("User name is invalid."));
            Assert.That(page.Message, Is.Null);
        }

        [Test]
        public async Task when_full_name_claim_is_rejected_marks_rollback_and_skips_update()
        {
            var user = NewUser();
            var userManager = CreateUserManager(user);
            userManager.Setup(m => m.AddClaimAsync(user, It.IsAny<Claim>())).ReturnsAsync(Rejected);
            var rollbackState = new TransactionRollbackState();
            var page = CreatePage(userManager, rollbackState, fullName: "New Name");

            await page.OnPostAsync(user.Id.ToString());

            Assert.That(rollbackState.IsRollbackOnly, Is.True);
            Assert.That(page.Message, Is.Null);
            userManager.Verify(m => m.UpdateAsync(It.IsAny<DesignerIdentityUser>()), Times.Never);
        }

        [Test]
        public async Task when_security_stamp_update_is_rejected_marks_rollback()
        {
            var user = NewUser();
            var userManager = CreateUserManager(user);
            userManager.Setup(m => m.UpdateSecurityStampAsync(user)).ReturnsAsync(Rejected);
            var rollbackState = new TransactionRollbackState();
            var page = CreatePage(userManager, rollbackState, isLockedOut: true);

            await page.OnPostAsync(user.Id.ToString());

            Assert.That(rollbackState.IsRollbackOnly, Is.True);
            Assert.That(page.Message, Is.Null);
        }

        [Test]
        public async Task when_all_identity_operations_succeed_does_not_mark_rollback()
        {
            var user = NewUser();
            var userManager = CreateUserManager(user);
            var rollbackState = new TransactionRollbackState();
            var page = CreatePage(userManager, rollbackState, fullName: "New Name", isLockedOut: true);

            await page.OnPostAsync(user.Id.ToString());

            Assert.That(rollbackState.IsRollbackOnly, Is.False);
            Assert.That(page.Message, Is.EqualTo("Account updated"));
        }

        private static DesignerIdentityUser NewUser() => new DesignerIdentityUser { Id = Guid.NewGuid(), Email = Email };

        private static Mock<UserManager<DesignerIdentityUser>> CreateUserManager(DesignerIdentityUser user)
        {
            var store = new Mock<IUserStore<DesignerIdentityUser>>();
            var userManager = new Mock<UserManager<DesignerIdentityUser>>(store.Object, null, null, null, null, null, null, null, null);
            userManager.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
            userManager.Setup(m => m.GetClaimsAsync(user)).ReturnsAsync(new List<Claim>());
            userManager.Setup(m => m.AddClaimAsync(user, It.IsAny<Claim>())).ReturnsAsync(IdentityResult.Success);
            userManager.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
            userManager.Setup(m => m.UpdateSecurityStampAsync(user)).ReturnsAsync(IdentityResult.Success);
            return userManager;
        }

        private static EditUserModel CreatePage(Mock<UserManager<DesignerIdentityUser>> userManager,
            ITransactionRollbackState rollbackState, string fullName = null, bool isLockedOut = false)
            => new EditUserModel(userManager.Object, rollbackState)
            {
                Input = new EditUserModel.InputModel
                {
                    Email = Email,
                    FullName = fullName,
                    IsLockedOut = isLockedOut
                },
                PageContext = new PageContext { HttpContext = new DefaultHttpContext() }
            };
    }
}
