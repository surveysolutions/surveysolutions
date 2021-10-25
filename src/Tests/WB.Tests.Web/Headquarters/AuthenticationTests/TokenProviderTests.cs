using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Headquarters.Code.Authentication;

namespace WB.Tests.Web.Headquarters.AuthenticationTests
{
    [TestOf(typeof(TokenProvider))]
    public class TokenProviderTests : TokenProviderTestContext
    {

        [Test]
        public static async Task when_validating_Jti_with_valid_value()
        {
            var testToken = "test_token";
            
            var userId = Guid.NewGuid();

            var user = Mock.Of<HqUser>();
            
            var userStore = new Mock<IUserStore<HqUser>>();
            userStore.Setup(u => u.FindByIdAsync(userId.FormatGuid(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(user));

            userStore.As<IUserPasswordStore<HqUser>>();
            
            var asIUserAuthenticationTokenStore = userStore.As<IUserAuthenticationTokenStore<HqUser>>();
            
            asIUserAuthenticationTokenStore.Setup(u => u.GetTokenAsync(user, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(testToken));
            
            
            var provider = TokenProvider(userStore.Object);

            var result = await provider.ValidateJtiAsync(user, testToken);
            Assert.IsTrue(result);
        }
        
        [Test]
        public static async Task when_validating_Jti_with_invalid_value()
        {
            var testToken = "test_token";
            
            var userId = Guid.NewGuid();

            var user = Mock.Of<HqUser>();
            
            var userStore = new Mock<IUserStore<HqUser>>();
            userStore.Setup(u => u.FindByIdAsync(userId.FormatGuid(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(user));

            userStore.As<IUserPasswordStore<HqUser>>();
            
            var asIUserAuthenticationTokenStore = userStore.As<IUserAuthenticationTokenStore<HqUser>>();
            
            asIUserAuthenticationTokenStore.Setup(u => u.GetTokenAsync(user, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(testToken));
            
            
            var provider = TokenProvider(userStore.Object);

            var result = await provider.ValidateJtiAsync(user, testToken + "11");
            Assert.IsFalse(result);
        }
        
        [Test]
        public static async Task when_invalidating_token()
        {
            var testToken = "test_token";
            
            var userId = Guid.NewGuid();

            var user = Mock.Of<HqUser>();
            
            var userStore = new Mock<IUserStore<HqUser>>();
            userStore.Setup(u => u.FindByIdAsync(userId.FormatGuid(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(user));

            userStore.As<IUserPasswordStore<HqUser>>();
            
            var asIUserAuthenticationTokenStore = userStore.As<IUserAuthenticationTokenStore<HqUser>>();
            
            asIUserAuthenticationTokenStore.Setup(u => u.GetTokenAsync(user, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(testToken));
            
            
            var provider = TokenProvider(userStore.Object);

            await provider.InvalidateBearerTokenAsync(user);
            
            asIUserAuthenticationTokenStore.Verify(x=> x.RemoveTokenAsync(user, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()));
        }
        
        [Test]
        public static async Task when_checking_token_existence_having_token()
        {
            var testToken = "test_token";
            
            var userId = Guid.NewGuid();

            var user = Mock.Of<HqUser>();
            
            var userStore = new Mock<IUserStore<HqUser>>();
            userStore.Setup(u => u.FindByIdAsync(userId.FormatGuid(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(user));

            userStore.As<IUserPasswordStore<HqUser>>();
            
            var asIUserAuthenticationTokenStore = userStore.As<IUserAuthenticationTokenStore<HqUser>>();
            
            asIUserAuthenticationTokenStore.Setup(u => u.GetTokenAsync(user, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(testToken));
            
            
            var provider = TokenProvider(userStore.Object);

            var result = await provider.DoesTokenExist(user);
            Assert.IsTrue(result);
        }
        
        [Test]
        public static async Task when_checking_token_existence_no_token()
        {
            var testToken = "";
            
            var userId = Guid.NewGuid();

            var user = Mock.Of<HqUser>();
            
            var userStore = new Mock<IUserStore<HqUser>>();
            userStore.Setup(u => u.FindByIdAsync(userId.FormatGuid(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(user));

            userStore.As<IUserPasswordStore<HqUser>>();
            
            var asIUserAuthenticationTokenStore = userStore.As<IUserAuthenticationTokenStore<HqUser>>();
            
            asIUserAuthenticationTokenStore.Setup(u => u.GetTokenAsync(user, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(testToken));
            
            
            var provider = TokenProvider(userStore.Object);

            var result = await provider.DoesTokenExist(user);
            Assert.IsFalse(result);
        }
        
        [Test]
        public static async Task when_generating_token()
        {
            
            var userId = Guid.NewGuid();
            var user = Mock.Of<HqUser>();
            
            var userStore = new Mock<IUserStore<HqUser>>();
            userStore.Setup(u => u.FindByIdAsync(userId.FormatGuid(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(user));

            var provider = TokenProvider(userStore.Object,
                Mock.Of<IOptions<TokenProviderOptions>>(c => c.Value == new TokenProviderOptions()
                {
                    Audience = "all",
                    Issuer = "Survey.Solutions",
                    IsBearerEnabled = true,
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(
                            Encoding.ASCII.GetBytes("test+password+for+signing")),
                        SecurityAlgorithms.HmacSha256)
                }));

            var result = await provider.GetOrCreateBearerTokenAsync(user);

            Assert.That(result, Is.Not.Empty);
        }
        
        [Test]
        public static async Task when_generating_token_with_turned_off_settings()
        {
            
            var userId = Guid.NewGuid();
            var user = Mock.Of<HqUser>();
            
            var userStore = new Mock<IUserStore<HqUser>>();
            userStore.Setup(u => u.FindByIdAsync(userId.FormatGuid(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(user));

            var provider = TokenProvider(userStore.Object,
                Mock.Of<IOptions<TokenProviderOptions>>(c => c.Value == new TokenProviderOptions()
                {
                    Audience = "all",
                    Issuer = "Survey.Solutions",
                    IsBearerEnabled = false,
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(
                            Encoding.ASCII.GetBytes("test+password+for+signing")),
                        SecurityAlgorithms.HmacSha256)
                }));

            var result = await provider.GetOrCreateBearerTokenAsync(user);

            Assert.That(result, Is.Empty);
        }
    }
}
