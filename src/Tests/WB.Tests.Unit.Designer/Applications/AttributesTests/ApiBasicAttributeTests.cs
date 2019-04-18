using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Services;

namespace WB.Tests.Unit.Designer.Applications.AttributesTests
{
    [TestFixture]
    internal class ApiBasicAttributeTests : AttributesTestContext
    {
        [SetUp]
        public void SetUp()
        {
            userMock.Setup(s => s.UserName).Returns(userName);
            userMock.Setup(s => s.EmailConfirmed).Returns(true);
            userMock.Setup(s => s.Email).Returns(userEmail);

            var emailStore = new Mock<IUserEmailStore<DesignerIdentityUser>>();
            var passwordStore = emailStore.As<IUserPasswordStore<DesignerIdentityUser>>();
            passwordStore.Setup(s => s.GetPasswordHashAsync(userMock.Object, It.IsAny<CancellationToken>()))
                .ReturnsAsync("hash");
            var userRoleStore = passwordStore.As<IUserRoleStore<DesignerIdentityUser>>();
            userRoleStore.Setup(s => s.GetRolesAsync(userMock.Object, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string>());
            userStoreMock = userRoleStore.As<IUserStore<DesignerIdentityUser>>();
            userStoreMock.Setup(r => r.FindByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(userMock.Object);

            allowedAddressServiceMock = new Mock<IAllowedAddressService>();
            ipAddressProviderMock = new Mock<IIpAddressProvider>();

            var authHeader = new StringValues("Basic " + EncodeToBase64($"{userName}:{"password"}"));
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
            filterContext = new AuthorizationFilterContext(actionContext, filters); 
        }

        [Test]
        public async Task When_authorizing_and_user_can_import_questionnaires_on_HQ()
        {
            userMock.Setup(s => s.CanImportOnHq).Returns(true);

            var attribute = CreateApiBasicAuthFilter(true, userStore: userStoreMock.Object);

            await attribute.OnAuthorizationAsync(filterContext);

            Assert.That(filterContext.HttpContext.User.Identity.Name, Is.EqualTo(userName));
        }

        [Test]
        public async Task When_authorizing_and_user_cannot_import_questionnaires_and_ip_is_unknown()
        {
            userMock.Setup(s => s.CanImportOnHq).Returns(false);
            ipAddressProviderMock.Setup(x => x.GetClientIpAddress()).Returns((IPAddress)null);
            allowedAddressServiceMock.Setup(x => x.IsAllowedAddress(null)).Returns(false);

            var attribute = CreateApiBasicAuthFilter(true, 
                userStore: userStoreMock.Object, 
                ipAddressProvider: ipAddressProviderMock.Object,
                allowedAddressService: allowedAddressServiceMock.Object);

            await attribute.OnAuthorizationAsync(filterContext);

            Assert.AreEqual(StatusCodes.Status403Forbidden, ((ContentResult)filterContext.Result).StatusCode);
        }

        [Test]
        public async Task When_authorizing_and_user_cannot_import_questionnaires_and_ip_is_allowed()
        {
            var ipAddress = IPAddress.Parse("65.87.163.24");

            userMock.Setup(s => s.CanImportOnHq).Returns(false);
            ipAddressProviderMock.Setup(x => x.GetClientIpAddress()).Returns(ipAddress);
            allowedAddressServiceMock.Setup(x => x.IsAllowedAddress(ipAddress)).Returns(true);

            var attribute = CreateApiBasicAuthFilter(true, 
                userStore: userStoreMock.Object, 
                ipAddressProvider: ipAddressProviderMock.Object,
                allowedAddressService: allowedAddressServiceMock.Object);

            await attribute.OnAuthorizationAsync(filterContext);

            Assert.AreEqual(userName, filterContext.HttpContext.User.Identity.Name);
        }

        [Test]
        public async Task When_authorizing_and_user_cannot_import_questionnaires_and_ip_is_not_allowed()
        {
            var ipAddress = IPAddress.Parse("65.87.163.24");

            userMock.Setup(s => s.CanImportOnHq).Returns(false);
            ipAddressProviderMock.Setup(x => x.GetClientIpAddress()).Returns(ipAddress);
            allowedAddressServiceMock.Setup(x => x.IsAllowedAddress(ipAddress)).Returns(false);

            var attribute = CreateApiBasicAuthFilter(true, 
                userStore: userStoreMock.Object, 
                ipAddressProvider: ipAddressProviderMock.Object,
                allowedAddressService: allowedAddressServiceMock.Object);

            await attribute.OnAuthorizationAsync(filterContext);

            Assert.AreEqual(StatusCodes.Status403Forbidden, ((ContentResult)filterContext.Result).StatusCode);
        }

        private static string userName = "name";
        private static string userEmail = "user@mail";
        private static Mock<DesignerIdentityUser> userMock = new Mock<DesignerIdentityUser>();
        private static AuthorizationFilterContext filterContext;
        private static Mock<IIpAddressProvider> ipAddressProviderMock;
        private static Mock<IAllowedAddressService> allowedAddressServiceMock;
        private static Mock<IUserStore<DesignerIdentityUser>> userStoreMock;
    }
}
