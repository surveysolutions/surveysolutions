using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using reCAPTCHA.AspNetCore;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.UI.Designer.Areas.Identity.Pages.Account;
using WB.UI.Shared.Web.Services;

namespace WB.Tests.Unit.Designer.Areas.Identity.Pages.Account
{
    [TestFixture]
    [TestOf(typeof(LoginModel))]
    public class LoginModelTests
    {
        [Test]
        public async Task when_login_succeeds_should_store_last_login_date()
        {
            var user = new DesignerIdentityUser { EmailConfirmed = true, UserName = "tester" };
            var userManagerMock = CreateUserManager();
            userManagerMock.Setup(x => x.FindByNameAsync("tester"))
                .ReturnsAsync(user);
            userManagerMock.Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            var signInManagerMock = CreateSignInManager(userManagerMock.Object);
            signInManagerMock.Setup(x => x.PasswordSignInAsync(user, "pwd", false, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            var model = CreateLoginModel(signInManagerMock.Object, userManagerMock.Object);
            var beforeLogin = DateTime.UtcNow;

            await model.OnPostAsync("/manage");
            var afterLogin = DateTime.UtcNow;

            userManagerMock.Verify(x => x.UpdateAsync(user), Times.Once);
            Assert.That(user.LastLoginAtUtc, Is.Not.Null);
            Assert.That(user.LastLoginAtUtc!.Value, Is.GreaterThanOrEqualTo(beforeLogin));
            Assert.That(user.LastLoginAtUtc!.Value, Is.LessThanOrEqualTo(afterLogin));
        }

        [Test]
        public async Task when_login_fails_should_not_store_last_login_date()
        {
            var user = new DesignerIdentityUser { EmailConfirmed = true, UserName = "tester" };
            var userManagerMock = CreateUserManager();
            userManagerMock.Setup(x => x.FindByNameAsync("tester"))
                .ReturnsAsync(user);

            var signInManagerMock = CreateSignInManager(userManagerMock.Object);
            signInManagerMock.Setup(x => x.PasswordSignInAsync(user, "pwd", false, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            var model = CreateLoginModel(signInManagerMock.Object, userManagerMock.Object);

            await model.OnPostAsync("/manage");

            userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<DesignerIdentityUser>()), Times.Never);
            Assert.That(user.LastLoginAtUtc, Is.Null);
        }

        [Test]
        public async Task when_login_requires_2fa_should_not_store_last_login_date_before_2fa_completion()
        {
            var user = new DesignerIdentityUser { EmailConfirmed = true, UserName = "tester" };
            var userManagerMock = CreateUserManager();
            userManagerMock.Setup(x => x.FindByNameAsync("tester"))
                .ReturnsAsync(user);

            var signInManagerMock = CreateSignInManager(userManagerMock.Object);
            signInManagerMock.Setup(x => x.PasswordSignInAsync(user, "pwd", false, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.TwoFactorRequired);

            var model = CreateLoginModel(signInManagerMock.Object, userManagerMock.Object);

            var result = await model.OnPostAsync("/manage");

            Assert.That(result, Is.InstanceOf<RedirectToPageResult>());
            userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<DesignerIdentityUser>()), Times.Never);
            Assert.That(user.LastLoginAtUtc, Is.Null);
        }

        private static LoginModel CreateLoginModel(SignInManager<DesignerIdentityUser> signInManager,
            UserManager<DesignerIdentityUser> userManager)
        {
            var urlHelper = new Mock<IUrlHelper>();
            urlHelper.Setup(x => x.IsLocalUrl(It.IsAny<string>()))
                .Returns(true);
            urlHelper.Setup(x => x.Content(It.IsAny<string>()))
                .Returns("/");

            var captchaService = new Mock<ICaptchaService>();
            captchaService.Setup(x => x.ShouldShowCaptcha(It.IsAny<string>()))
                .Returns(false);

            var model = new LoginModel(
                signInManager,
                userManager,
                Mock.Of<ILogger<LoginModel>>(),
                captchaService.Object,
                Mock.Of<IRecaptchaService>())
            {
                Input = new LoginModel.InputModel
                {
                    Email = "tester",
                    Password = "pwd"
                },
                Url = urlHelper.Object,
                PageContext = new PageContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            return model;
        }

        private static Mock<UserManager<DesignerIdentityUser>> CreateUserManager()
        {
            var store = new Mock<IUserStore<DesignerIdentityUser>>();
            return new Mock<UserManager<DesignerIdentityUser>>(store.Object, null, null, null, null, null, null, null, null);
        }

        private static Mock<SignInManager<DesignerIdentityUser>> CreateSignInManager(UserManager<DesignerIdentityUser> userManager)
        {
            return new Mock<SignInManager<DesignerIdentityUser>>(
                userManager,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<DesignerIdentityUser>>(),
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<ILogger<SignInManager<DesignerIdentityUser>>>(),
                Mock.Of<IAuthenticationSchemeProvider>(),
                Mock.Of<IUserConfirmation<DesignerIdentityUser>>());
        }
    }
}
