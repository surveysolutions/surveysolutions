using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users.UserProfile;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Infrastructure.Native.Workspaces;
using WB.Tests.Abc;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Models.Users;

namespace WB.Tests.Unit.Applications.Headquarters
{
    [TestFixture]
    [TestOf(typeof(UsersController))]
    public class UsersControllerTests
    {
        [Test]
        public void when_UpdatePassword_by_admin_should_set_flag_PasswordChangeRequired()
        {
            var userId = Guid.NewGuid();

            var user = Mock.Of<HqUser>();
            var authorizedUser = Mock.Of<IAuthorizedUser>(u =>
                u.IsAdministrator == true);
            var userManager = new Mock<IUserStore<HqUser>>();
            userManager.Setup(u => u.FindByIdAsync(userId.FormatGuid(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(user));
            userManager.Setup(u => u.UpdateAsync(user, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new IdentityResult()));
            userManager.As<IUserPasswordStore<HqUser>>();
            
            var controller = CreateController(userManager.Object, authorizedUser);

            controller.UpdatePassword(new ChangePasswordModel()
            {
                Password = "11",
                ConfirmPassword = "11",
                UserId = userId,
                OldPassword = "1"
            });

            Assert.That(user.PasswordChangeRequired, Is.True);
        }

        [Test]
        public void when_UpdatePassword_by_own_user_should_not_set_flag_PasswordChangeRequired()
        {
            var userId = Guid.NewGuid();

            var user = Mock.Of<HqUser>();
            var authorizedUser = Mock.Of<IAuthorizedUser>(u => u.Id == userId);
            var userManager = new Mock<IUserStore<HqUser>>();
            userManager.Setup(u => u.FindByIdAsync(userId.FormatGuid(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(user));
            userManager.Setup(u => u.UpdateAsync(user, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new IdentityResult()));
            userManager.As<IUserPasswordStore<HqUser>>();
            
            var controller = CreateController(userManager.Object, authorizedUser);

            controller.UpdatePassword(new ChangePasswordModel()
            {
                Password = "11",
                ConfirmPassword = "11",
                UserId = userId,
                OldPassword = "1"
            });

            Assert.That(user.PasswordChangeRequired, Is.False);
        }

        private UsersController CreateController(
            IUserStore<HqUser> userManager = null,
            IAuthorizedUser authorizedUser = null)
        {
            var controller = new UsersController(
                authorizedUser ?? Mock.Of<IAuthorizedUser>(),
                CreateHqUserManager(userManager),
                Mock.Of<IPlainKeyValueStorage<ProfileSettings>>(),
                Mock.Of<UrlEncoder>(),
                Mock.Of<IOptions<HeadquartersConfig>>(),
                null,
                Mock.Of<SignInManager<HqUser>>());
            controller.ControllerContext.HttpContext = Mock.Of<HttpContext>(c => 
                c.Session == new MockHttpSession()
                && c.Request == Mock.Of<HttpRequest>(r => r.Cookies == Mock.Of<IRequestCookieCollection>())
                && c.Response == Mock.Of<HttpResponse>(r => r.Cookies == Mock.Of<IResponseCookies>()));
            controller.Url = Mock.Of<IUrlHelper>(x => x.Action(It.IsAny<UrlActionContext>()) == "url");

            return controller;
        }

        private HqUserManager CreateHqUserManager(IUserStore<HqUser> userStore)
        {
            var hqUserManager = new HqUserManager(
                userStore ?? new Mock<IUserStore<HqUser>>().As<IUserPasswordStore<HqUser>>().Object,
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<IPasswordHasher<HqUser>>(),
                new List<IUserValidator<HqUser>>(),
                new List<IPasswordValidator<HqUser>>(),
                Mock.Of<ILookupNormalizer>(),
                Mock.Of<IdentityErrorDescriber>(),
                Mock.Of<IServiceProvider>(),
                Mock.Of<ILogger<HqUserManager>>(),
                Mock.Of<IWorkspacesService>(),
                Mock.Of<ISystemLog>(),
                Mock.Of<IAuthorizedUser>());
            hqUserManager.RegisterTokenProvider(
                hqUserManager.Options.Tokens.PasswordResetTokenProvider, 
                Mock.Of<IUserTwoFactorTokenProvider<HqUser>>(v => 
                    v.ValidateAsync(It.IsAny<string>(), It.IsAny<string>(), hqUserManager,
                        It.IsAny<HqUser>()) == Task.FromResult(true)));
            return hqUserManager;
        }
    }
}