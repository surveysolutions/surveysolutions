#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Headquarters.Code.Authentication;

namespace WB.Tests.Web.Headquarters.AuthenticationTests
{
    public class TokenProviderTestContext
    {
        public static TokenProvider TokenProvider(IUserStore<HqUser>? userStore = null, IOptions<TokenProviderOptions>? options = null)
        {
            if (userStore == null)
            {
                var userId = Guid.NewGuid();

                var user = Mock.Of<HqUser>();
            
                var userStoreMock = new Mock<IUserStore<HqUser>>();
                userStoreMock.Setup(u => u.FindByIdAsync(userId.FormatGuid(), It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(user));
                userStoreMock.Setup(u => u.UpdateAsync(user, It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(new IdentityResult()));

                userStoreMock.As<IUserPasswordStore<HqUser>>();

                userStore = userStoreMock.Object;
            }

            return new TokenProvider(
                options ?? Mock.Of<IOptions<TokenProviderOptions>>(),
                CreateHqUserManager(userStore));
        }

        private static HqUserManager CreateHqUserManager(IUserStore<HqUser>? userStore)
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
