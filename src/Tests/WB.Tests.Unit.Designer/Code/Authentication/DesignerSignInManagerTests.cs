#nullable enable
using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.UI.Designer.Code.Authentication;

namespace WB.Tests.Unit.Designer.Code.Authentication
{
    [TestFixture]
    [TestOf(typeof(DesignerSignInManager))]
    public class DesignerSignInManagerTests
    {
        [Test]
        public async Task when_two_factor_authenticator_sign_in_succeeds_should_update_last_login_at_utc()
        {
            var user = new DesignerIdentityUser { UserName = "tester" };
            var userManagerMock = CreateUserManagerMock();
            userManagerMock.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            var manager = CreateTestableManager(userManagerMock.Object, SignInResult.Success, user);
            var before = DateTime.UtcNow;

            await manager.TwoFactorAuthenticatorSignInAsync("code", false, false);

            var after = DateTime.UtcNow;
            userManagerMock.Verify(x => x.UpdateAsync(user), Times.Once);
            user.LastLoginAtUtc.Should().NotBeNull();
            user.LastLoginAtUtc!.Value.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        }

        [Test]
        public async Task when_two_factor_authenticator_sign_in_fails_should_not_update_last_login_at_utc()
        {
            var user = new DesignerIdentityUser { UserName = "tester" };
            var userManagerMock = CreateUserManagerMock();

            var manager = CreateTestableManager(userManagerMock.Object, SignInResult.Failed, user);

            await manager.TwoFactorAuthenticatorSignInAsync("code", false, false);

            userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<DesignerIdentityUser>()), Times.Never);
            user.LastLoginAtUtc.Should().BeNull();
        }

        [Test]
        public async Task when_two_factor_recovery_code_sign_in_succeeds_should_update_last_login_at_utc()
        {
            var user = new DesignerIdentityUser { UserName = "tester" };
            var userManagerMock = CreateUserManagerMock();
            userManagerMock.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            var manager = CreateTestableManager(userManagerMock.Object, SignInResult.Success, user);
            var before = DateTime.UtcNow;

            await manager.TwoFactorRecoveryCodeSignInAsync("recovery-code");

            var after = DateTime.UtcNow;
            userManagerMock.Verify(x => x.UpdateAsync(user), Times.Once);
            user.LastLoginAtUtc.Should().NotBeNull();
            user.LastLoginAtUtc!.Value.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        }

        [Test]
        public async Task when_two_factor_recovery_code_sign_in_fails_should_not_update_last_login_at_utc()
        {
            var user = new DesignerIdentityUser { UserName = "tester" };
            var userManagerMock = CreateUserManagerMock();

            var manager = CreateTestableManager(userManagerMock.Object, SignInResult.Failed, user);

            await manager.TwoFactorRecoveryCodeSignInAsync("recovery-code");

            userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<DesignerIdentityUser>()), Times.Never);
            user.LastLoginAtUtc.Should().BeNull();
        }

        [Test]
        public async Task when_two_factor_provider_sign_in_succeeds_should_update_last_login_at_utc()
        {
            var user = new DesignerIdentityUser { UserName = "tester" };
            var userManagerMock = CreateUserManagerMock();
            userManagerMock.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            var manager = CreateTestableManager(userManagerMock.Object, SignInResult.Success, user);
            var before = DateTime.UtcNow;

            await manager.TwoFactorSignInAsync("Email", "code", false, false);

            var after = DateTime.UtcNow;
            userManagerMock.Verify(x => x.UpdateAsync(user), Times.Once);
            user.LastLoginAtUtc.Should().NotBeNull();
            user.LastLoginAtUtc!.Value.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        }

        [Test]
        public async Task when_two_factor_provider_sign_in_fails_should_not_update_last_login_at_utc()
        {
            var user = new DesignerIdentityUser { UserName = "tester" };
            var userManagerMock = CreateUserManagerMock();

            var manager = CreateTestableManager(userManagerMock.Object, SignInResult.Failed, user);

            await manager.TwoFactorSignInAsync("Email", "code", false, false);

            userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<DesignerIdentityUser>()), Times.Never);
            user.LastLoginAtUtc.Should().BeNull();
        }

        private static TestableDesignerSignInManager CreateTestableManager(
            UserManager<DesignerIdentityUser> userManager,
            SignInResult baseResult,
            DesignerIdentityUser twoFactorUser)
        {
            return new TestableDesignerSignInManager(
                userManager,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<DesignerIdentityUser>>(),
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<ILogger<SignInManager<DesignerIdentityUser>>>(),
                Mock.Of<IAuthenticationSchemeProvider>(),
                Mock.Of<IUserConfirmation<DesignerIdentityUser>>(),
                baseResult,
                twoFactorUser);
        }

        private static Mock<UserManager<DesignerIdentityUser>> CreateUserManagerMock()
        {
            var store = new Mock<IUserStore<DesignerIdentityUser>>();
            return new Mock<UserManager<DesignerIdentityUser>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        }

        private class TestableDesignerSignInManager : DesignerSignInManager
        {
            private readonly SignInResult _baseResult;
            private readonly DesignerIdentityUser _twoFactorUser;

            public TestableDesignerSignInManager(
                UserManager<DesignerIdentityUser> userManager,
                IHttpContextAccessor contextAccessor,
                IUserClaimsPrincipalFactory<DesignerIdentityUser> claimsFactory,
                IOptions<IdentityOptions> optionsAccessor,
                ILogger<SignInManager<DesignerIdentityUser>> logger,
                IAuthenticationSchemeProvider schemes,
                IUserConfirmation<DesignerIdentityUser> confirmation,
                SignInResult baseResult,
                DesignerIdentityUser twoFactorUser)
                : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
            {
                _baseResult = baseResult;
                _twoFactorUser = twoFactorUser;
            }

            protected override Task<SignInResult> BaseTwoFactorAuthenticatorSignInAsync(
                string code, bool isPersistent, bool rememberClient)
                => Task.FromResult(_baseResult);

            protected override Task<SignInResult> BaseTwoFactorRecoveryCodeSignInAsync(string recoveryCode)
                => Task.FromResult(_baseResult);

            protected override Task<SignInResult> BaseTwoFactorSignInAsync(
                string provider, string code, bool isPersistent, bool rememberClient)
                => Task.FromResult(_baseResult);

            public override Task<DesignerIdentityUser?> GetTwoFactorAuthenticationUserAsync()
                => Task.FromResult<DesignerIdentityUser?>(_twoFactorUser);
        }
    }
}
