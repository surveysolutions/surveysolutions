using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Services.Accounts;

namespace WB.Tests.Unit.Designer.Applications.AttributesTests
{
    [TestFixture]
    internal class ApiBasicAttributeTests : AttributesTestContext
    {
        [SetUp]
        public void SetUp()
        {
            AssemblyContext.SetupServiceLocator();

            var membershipUserServiceMock = new Mock<IMembershipUserService>();
            var membershipWebUserMock = new Mock<IMembershipWebUser>();
            membershipUserMock.Setup(x => x.IsApproved).Returns(true);
            membershipUserMock.Setup(x => x.Email).Returns(userEmail);
            membershipWebUserMock.Setup(x => x.MembershipUser).Returns(membershipUserMock.Object);
            membershipUserServiceMock.Setup(_ => _.WebUser).Returns(membershipWebUserMock.Object);

            allowedAddressServiceMock = new Mock<IAllowedAddressService>();
            ipAddressProviderMock = new Mock<IIpAddressProvider>();

            var requestMessage = new HttpRequestMessage();
            requestMessage.RequestUri = new Uri("http://www.example.com");
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", EncodeToBase64($"{userName}:{"password"}"));

            var context = new Mock<HttpConfiguration>();
            var actionDescriptor = new Mock<HttpActionDescriptor>();
            var controllerContext = new HttpControllerContext(context.Object, new HttpRouteData(new HttpRoute()), requestMessage);
            actionContext = new HttpActionContext(controllerContext, actionDescriptor.Object);

            Setup.InstanceToMockedServiceLocator<IIpAddressProvider>(ipAddressProviderMock.Object);
            Setup.InstanceToMockedServiceLocator<IMembershipUserService>(membershipUserServiceMock.Object);
            Setup.InstanceToMockedServiceLocator<IAllowedAddressService>(allowedAddressServiceMock.Object);
            Setup.InstanceToMockedServiceLocator<IAccountRepository>(
                Mock.Of<IAccountRepository>(x => x.GetByNameOrEmail(userName) == Mock.Of<IMembershipAccount>(a => a.UserName == userName)));
        }

        [Test]
        public void When_authorizing_and_user_can_import_questionnaires_on_HQ()
        {
            membershipUserMock.Setup(x => x.CanImportOnHq).Returns(true);

            var attribute = CreateApiBasicAuthAttribute((s, s1) => true, true);

            attribute.OnAuthorization(actionContext);

            Assert.That(Thread.CurrentPrincipal.Identity.Name, Is.EqualTo(userName));
        }

        [Test]
        public void When_authorizing_and_user_cannot_import_questionnaires_and_ip_is_unknown()
        {
            membershipUserMock.Setup(x => x.CanImportOnHq).Returns(false);
            ipAddressProviderMock.Setup(x => x.GetClientIpAddress()).Returns((IPAddress)null);
            allowedAddressServiceMock.Setup(x => x.IsAllowedAddress(null)).Returns(false);

            var attribute = CreateApiBasicAuthAttribute((s, s1) => true, true);

            attribute.OnAuthorization(actionContext);

            Assert.AreEqual(HttpStatusCode.Unauthorized, actionContext.Response.StatusCode);
        }

        [Test]
        public void When_authorizing_and_user_cannot_import_questionnaires_and_ip_is_allowed()
        {
            var ipAddress = IPAddress.Parse("65.87.163.24");

            membershipUserMock.Setup(x => x.CanImportOnHq).Returns(false);
            ipAddressProviderMock.Setup(x => x.GetClientIpAddress()).Returns(ipAddress);
            allowedAddressServiceMock.Setup(x => x.IsAllowedAddress(ipAddress)).Returns(true);

            var attribute = CreateApiBasicAuthAttribute((s, s1) => true, true);

            attribute.OnAuthorization(actionContext);

            Assert.AreEqual(userName, Thread.CurrentPrincipal.Identity.Name);
        }

        [Test]
        public void When_authorizing_and_user_cannot_import_questionnaires_and_ip_is_not_allowed()
        {
            var ipAddress = IPAddress.Parse("65.87.163.24");

            membershipUserMock.Setup(x => x.CanImportOnHq).Returns(false);
            ipAddressProviderMock.Setup(x => x.GetClientIpAddress()).Returns(ipAddress);
            allowedAddressServiceMock.Setup(x => x.IsAllowedAddress(ipAddress)).Returns(false);

            var attribute = CreateApiBasicAuthAttribute((s, s1) => true, true);

            attribute.OnAuthorization(actionContext);

            Assert.AreEqual(HttpStatusCode.Unauthorized, actionContext.Response.StatusCode);
        }

        private static string userName = "name";
        private static string userEmail = "user@mail";
        private static readonly Mock<DesignerMembershipUser> membershipUserMock = new Mock<DesignerMembershipUser>();
        private static HttpActionContext actionContext;
        private static Mock<IIpAddressProvider> ipAddressProviderMock;
        private static Mock<IAllowedAddressService> allowedAddressServiceMock;
    }
}
