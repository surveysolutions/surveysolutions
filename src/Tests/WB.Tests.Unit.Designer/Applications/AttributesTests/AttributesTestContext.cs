using System;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Services;
using WB.UI.Designer.Code.Attributes;

namespace WB.Tests.Unit.Designer.Applications.AttributesTests
{
    internal class AttributesTestContext
    {
        public static ApiBasicAuthFilter CreateApiBasicAuthFilter(bool onlyAllowedAddresses,
            IIpAddressProvider ipAddressProvider = null, 
            IAllowedAddressService allowedAddressService = null,
            IUserStore<DesignerIdentityUser> userStore = null)
        {
            var userManager = new UserManager<DesignerIdentityUser>(
                userStore ?? new Mock<IUserEmailStore<DesignerIdentityUser>>().As<IUserStore<DesignerIdentityUser>>().Object,
                null, 
                Mock.Of<IPasswordHasher<DesignerIdentityUser>>(ph => ph.VerifyHashedPassword(It.IsAny<DesignerIdentityUser>(), It.IsAny<string>(), It.IsAny<string>()) == PasswordVerificationResult.Success)
                , null, null, null, null, null, 
                Mock.Of<ILogger<UserManager<DesignerIdentityUser>>>());
            var signInManager = new SignInManager<DesignerIdentityUser>(
                userManager, 
                Mock.Of<IHttpContextAccessor>(), 
                Mock.Of<IUserClaimsPrincipalFactory<DesignerIdentityUser>>(), 
                null, 
                Mock.Of<ILogger<SignInManager<DesignerIdentityUser>>>(),
                null);
            return new ApiBasicAuthFilter(onlyAllowedAddresses,
                ipAddressProvider ?? Mock.Of<IIpAddressProvider>(), 
                allowedAddressService ?? Mock.Of<IAllowedAddressService>(),
                userManager,
                signInManager);
        }
        
        public static string EncodeToBase64(string value)
        {
            byte[] toEncodeAsBytes = Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(toEncodeAsBytes);
        }
    }
}
