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
            userMock = new Mock<DesignerIdentityUser>();
            userMock.Setup(s => s.UserName).Returns(userName);
            userMock.Setup(s => s.EmailConfirmed).Returns(true);
            userMock.Setup(s => s.Email).Returns(userEmail);

            allowedAddressServiceMock = new Mock<IAllowedAddressService>();
            ipAddressProviderMock = new Mock<IIpAddressProvider>();

            userStore = CreateAndSetupUserStore(userMock.Object);
            filterContext = CreateAndSetupActionFilterContext(userName);
        }

        [Test]
        public async Task When_authorizing_and_user_can_import_questionnaires_on_HQ()
        {
            userMock.Setup(s => s.CanImportOnHq).Returns(true);

            var attribute = CreateApiBasicAuthFilter(onlyAllowedAddresses: true, userStore: userStore);

            await attribute.OnAuthorizationAsync(filterContext);

            Assert.That(filterContext.HttpContext.User.Identity.Name, Is.EqualTo(userName));
        }

        [Test]
        public async Task When_authorizing_and_user_cannot_import_questionnaires_and_ip_is_unknown()
        {
            userMock.Setup(s => s.CanImportOnHq).Returns(false);
            ipAddressProviderMock.Setup(x => x.GetClientIpAddress()).Returns((IPAddress)null);
            allowedAddressServiceMock.Setup(x => x.IsAllowedAddress(null)).Returns(false);

            var attribute = CreateApiBasicAuthFilter(onlyAllowedAddresses: true, 
                userStore: userStore, 
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

            var attribute = CreateApiBasicAuthFilter(onlyAllowedAddresses: true, 
                userStore: userStore, 
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

            var attribute = CreateApiBasicAuthFilter(onlyAllowedAddresses: true, 
                userStore: userStore, 
                ipAddressProvider: ipAddressProviderMock.Object,
                allowedAddressService: allowedAddressServiceMock.Object);

            await attribute.OnAuthorizationAsync(filterContext);

            Assert.AreEqual(StatusCodes.Status403Forbidden, ((ContentResult)filterContext.Result).StatusCode);
        }

        private static string userName = "name";
        private static string userEmail = "user@mail";
        private static Mock<DesignerIdentityUser> userMock;
        private static AuthorizationFilterContext filterContext;
        private static Mock<IIpAddressProvider> ipAddressProviderMock;
        private static Mock<IAllowedAddressService> allowedAddressServiceMock;
        private static IUserStore<DesignerIdentityUser> userStore;
    }
}
