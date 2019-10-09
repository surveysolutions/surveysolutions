using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Services;
using WB.UI.Designer.Code.Attributes;

namespace WB.Tests.Unit.Designer.Applications.AttributesTests
{
    internal class AttributesTestContext
    {
        public static IpAddressFilter CreateApiBasicAuthFilter(
            IUserStore<DesignerIdentityUser> userStore = null,
            IIpAddressProvider ipAddressProvider = null, 
            IAllowedAddressService allowedAddressService = null,
            bool onlyAllowedAddresses = false)
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
            return new IpAddressFilter(onlyAllowedAddresses,
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

        public static IUserStore<DesignerIdentityUser> CreateAndSetupEmptyUserStore()
        {
            var emailStore = new Mock<IUserEmailStore<DesignerIdentityUser>>();
            var passwordStore = emailStore.As<IUserPasswordStore<DesignerIdentityUser>>();
            var userRoleStore = passwordStore.As<IUserRoleStore<DesignerIdentityUser>>();
            var userStoreMock = userRoleStore.As<IUserStore<DesignerIdentityUser>>();
            return userStoreMock.Object;
        }

        public static IUserStore<DesignerIdentityUser> CreateAndSetupUserStore(DesignerIdentityUser user)
        {
            var emailStore = new Mock<IUserEmailStore<DesignerIdentityUser>>();
            emailStore.Setup(s => s.GetEmailAsync(user, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user.Email);
            var passwordStore = emailStore.As<IUserPasswordStore<DesignerIdentityUser>>();
            passwordStore.Setup(s => s.GetPasswordHashAsync(user, It.IsAny<CancellationToken>()))
                .ReturnsAsync("hash");
            var userRoleStore = passwordStore.As<IUserRoleStore<DesignerIdentityUser>>();
            userRoleStore.Setup(s => s.GetRolesAsync(user, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string>());
            var userStoreMock = userRoleStore.As<IUserStore<DesignerIdentityUser>>();
            userStoreMock.Setup(r => r.FindByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            return userStoreMock.Object;
        }

        public static AuthorizationFilterContext CreateAndSetupActionFilterContext(string userName, string password = "password")
        {
            var authHeader = new StringValues("Basic " + EncodeToBase64($"{userName}:{password}"));
            IHeaderDictionary headers = Mock.Of<IHeaderDictionary>(h => h["Authorization"] == authHeader);
            HttpRequest request = Mock.Of<HttpRequest>(r => r.Headers == headers);
            IHeaderDictionary responseHeaders = Mock.Of<IHeaderDictionary>();
            HttpResponse response = Mock.Of<HttpResponse>(r => r.Headers == responseHeaders);
            HttpContext httpContext = Mock.Of<HttpContext>(c => c.Request == request && c.Response == response);
            ActionDescriptor actionDescriptor = Mock.Of<ActionDescriptor>();
            RouteData routeData = Mock.Of<RouteData>();
            ActionContext actionContext = Mock.Of<ActionContext>(ac => 
                ac.HttpContext == httpContext
                && ac.ActionDescriptor == actionDescriptor
                && ac.RouteData == routeData
            );
            IList<IFilterMetadata> filters = Mock.Of<IList<IFilterMetadata>>();
            var filterContext = new AuthorizationFilterContext(actionContext, filters);
            return filterContext;
        }
        
        public static AuthorizationFilterContext CreateAndSetupActionFilterContextWithoutAuthorization()
        {
            IHeaderDictionary requestHeaders = Mock.Of<IHeaderDictionary>();
            HttpRequest request = Mock.Of<HttpRequest>(r => r.Headers == requestHeaders);
            IHeaderDictionary responseHeaders = Mock.Of<IHeaderDictionary>();
            HttpResponse response = Mock.Of<HttpResponse>(r => r.Headers == responseHeaders);
            HttpContext httpContext = Mock.Of<HttpContext>(c => c.Request == request && c.Response == response);
            ActionDescriptor actionDescriptor = Mock.Of<ActionDescriptor>();
            RouteData routeData = Mock.Of<RouteData>();
            ActionContext actionContext = Mock.Of<ActionContext>(ac => 
                ac.HttpContext == httpContext
                && ac.ActionDescriptor == actionDescriptor
                && ac.RouteData == routeData
            );
            IList<IFilterMetadata> filters = Mock.Of<IList<IFilterMetadata>>();
            var filterContext = new AuthorizationFilterContext(actionContext, filters);
            return filterContext;
        }
    }
}
