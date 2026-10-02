#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
using WB.UI.Designer.Code.Attributes;
using WB.UI.Designer.Resources;
using WB.UI.Shared.Web.Authentication;

namespace WB.Tests.Unit.Designer.Code.Attributes
{
    [TestFixture]
    [TestOf(typeof(BasicBasicAuthenticationService))]
    public class BasicAuthenticationServiceTests
    {
        private Mock<UserManager<DesignerIdentityUser>> userManager = null!;
        private Mock<SignInManager<DesignerIdentityUser>> signInManager = null!;
        private BasicBasicAuthenticationService service = null!;

        [SetUp]
        public void Setup()
        {
            userManager = new Mock<UserManager<DesignerIdentityUser>>(
                new Mock<IUserStore<DesignerIdentityUser>>().Object,
                null!, null!, null!, null!, null!, null!, null!, null!);
            signInManager = new Mock<SignInManager<DesignerIdentityUser>>(
                userManager.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<DesignerIdentityUser>>(),
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<ILogger<SignInManager<DesignerIdentityUser>>>(),
                Mock.Of<IAuthenticationSchemeProvider>(),
                Mock.Of<IUserConfirmation<DesignerIdentityUser>>());
            service = new BasicBasicAuthenticationService(userManager.Object, signInManager.Object);
        }

        private static DesignerIdentityUser User(bool confirmed = true) => new DesignerIdentityUser
        {
            Id = Guid.NewGuid(),
            UserName = "user",
            Email = "user@example.org",
            EmailConfirmed = confirmed
        };

        private void SetupUser(DesignerIdentityUser user, bool passwordOk = true, string[]? roles = null)
        {
            userManager.Setup(x => x.FindByNameAsync("user")).ReturnsAsync(user);
            userManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(roles ?? Array.Empty<string>());
            userManager.Setup(x => x.GetEmailAsync(user)).ReturnsAsync(user.Email);
            signInManager.Setup(x => x.CheckPasswordSignInAsync(user, "pwd", true))
                .ReturnsAsync(passwordOk ? SignInResult.Success : SignInResult.Failed);
        }

        [Test]
        public void should_throw_when_constructed_with_null_dependencies()
        {
            Assert.Throws<ArgumentNullException>(() => new BasicBasicAuthenticationService(null!, signInManager.Object));
            Assert.Throws<ArgumentNullException>(() => new BasicBasicAuthenticationService(userManager.Object, null!));
        }

        [Test]
        public async Task should_return_principal_with_claims_for_valid_credentials()
        {
            var user = User();
            SetupUser(user, roles: new[] { "Administrator", "User" });

            var principal = await service.AuthenticateAsync(new BasicCredentials("user", "pwd"));

            principal.Identity!.Name.Should().Be("user");
            principal.Identity.AuthenticationType.Should().Be("Basic");
            principal.FindFirst(ClaimTypes.NameIdentifier)!.Value.Should().Be(user.Id.ToString());
            principal.FindFirst(ClaimTypes.Email)!.Value.Should().Be("user@example.org");
            principal.FindAll(ClaimTypes.Role).Select(x => x.Value).Should().BeEquivalentTo("Administrator", "User");
        }

        [Test]
        public async Task should_pass_lockout_on_failure_flag_when_checking_password()
        {
            var user = User();
            SetupUser(user);

            await service.AuthenticateAsync(new BasicCredentials("user", "pwd"));

            signInManager.Verify(x => x.CheckPasswordSignInAsync(user, "pwd", true), Times.Once);
        }

        [Test]
        public async Task should_fall_back_to_find_by_email_when_user_name_not_found()
        {
            var user = User();
            userManager.Setup(x => x.FindByNameAsync("user@example.org")).ReturnsAsync((DesignerIdentityUser?)null);
            userManager.Setup(x => x.FindByEmailAsync("user@example.org")).ReturnsAsync(user);
            userManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string>());
            userManager.Setup(x => x.GetEmailAsync(user)).ReturnsAsync(user.Email);
            signInManager.Setup(x => x.CheckPasswordSignInAsync(user, "pwd", true)).ReturnsAsync(SignInResult.Success);

            var principal = await service.AuthenticateAsync(new BasicCredentials("user@example.org", "pwd"));

            principal.Identity!.Name.Should().Be("user");
        }

        [Test]
        public async Task should_not_add_email_claim_when_email_is_null()
        {
            var user = User();
            SetupUser(user);
            userManager.Setup(x => x.GetEmailAsync(user)).ReturnsAsync((string?)null);

            var principal = await service.AuthenticateAsync(new BasicCredentials("user", "pwd"));

            principal.FindFirst(ClaimTypes.Email).Should().BeNull();
        }

        [Test]
        public async Task should_throw_401_when_user_not_found()
        {
            userManager.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync((DesignerIdentityUser?)null);
            userManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((DesignerIdentityUser?)null);

            var ex = Assert.ThrowsAsync<UnauthorizedException>(
                () => service.AuthenticateAsync(new BasicCredentials("nobody", "pwd")));

            ex!.ResponseStatusCode.Should().Be(StatusCodes.Status401Unauthorized);
            ex.Message.Should().Be(ErrorMessages.User_Not_authorized);
        }

        [Test]
        public void should_throw_401_when_user_has_no_user_name()
        {
            var user = User();
            user.UserName = null;
            userManager.Setup(x => x.FindByNameAsync("user")).ReturnsAsync(user);

            var ex = Assert.ThrowsAsync<UnauthorizedException>(
                () => service.AuthenticateAsync(new BasicCredentials("user", "pwd")));

            ex!.Message.Should().Be(ErrorMessages.User_Not_authorized);
        }

        [Test]
        public void should_throw_401_when_password_is_wrong()
        {
            SetupUser(User(), passwordOk: false);

            var ex = Assert.ThrowsAsync<UnauthorizedException>(
                () => service.AuthenticateAsync(new BasicCredentials("user", "pwd")));

            ex!.ResponseStatusCode.Should().Be(StatusCodes.Status401Unauthorized);
            ex.Message.Should().Be(ErrorMessages.User_Not_authorized);
        }

        [Test]
        public void should_throw_locked_out_when_lockout_is_active()
        {
            var user = User();
            user.LockoutEnabled = true;
            user.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(10);
            SetupUser(user);

            var ex = Assert.ThrowsAsync<UnauthorizedException>(
                () => service.AuthenticateAsync(new BasicCredentials("user", "pwd")));

            ex!.Message.Should().Be(ErrorMessages.UserLockedOut);
            ex.ResponseStatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        }

        [Test]
        public async Task should_not_treat_expired_lockout_as_locked()
        {
            var user = User();
            user.LockoutEnabled = true;
            user.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(-1);
            SetupUser(user);

            var principal = await service.AuthenticateAsync(new BasicCredentials("user", "pwd"));

            principal.Identity!.IsAuthenticated.Should().BeTrue();
        }

        [Test]
        public async Task should_ignore_lockout_end_when_lockout_is_disabled()
        {
            var user = User();
            user.LockoutEnabled = false;
            user.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(10);
            SetupUser(user);

            var principal = await service.AuthenticateAsync(new BasicCredentials("user", "pwd"));

            principal.Identity!.IsAuthenticated.Should().BeTrue();
        }

        [Test]
        public void should_throw_not_approved_with_email_when_email_not_confirmed()
        {
            SetupUser(User(confirmed: false));

            var ex = Assert.ThrowsAsync<UnauthorizedException>(
                () => service.AuthenticateAsync(new BasicCredentials("user", "pwd")));

            ex!.Message.Should().Be(string.Format(ErrorMessages.UserNotApproved, "user@example.org"));
            ex.ResponseStatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        }

        [Test]
        public void should_check_lockout_before_approval()
        {
            var user = User(confirmed: false);
            user.LockoutEnabled = true;
            user.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(10);
            SetupUser(user);

            var ex = Assert.ThrowsAsync<UnauthorizedException>(
                () => service.AuthenticateAsync(new BasicCredentials("user", "pwd")));

            ex!.Message.Should().Be(ErrorMessages.UserLockedOut);
        }
    }
}

