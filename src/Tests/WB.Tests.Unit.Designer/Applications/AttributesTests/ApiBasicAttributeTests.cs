using System;
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
using WB.Core.BoundedContexts.Designer.Views.AllowedAddresses;

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

            var allowedAddressServiceMock = new Mock<IAllowedAddressService>();

            Setup.InstanceToMockedServiceLocator<IMembershipUserService>(membershipUserServiceMock.Object);
            Setup.InstanceToMockedServiceLocator<IAllowedAddressService>(allowedAddressServiceMock.Object);
        }

        [Test]
        public void When_authorizing_and_user_can_import_questionnaires_on_HQ()
        {
            membershipUserMock.Setup(x => x.CanImportOnHq).Returns(true);
            var context = new Mock<HttpConfiguration>();

            var requestMessage = new HttpRequestMessage();
            requestMessage.RequestUri = new Uri("http://www.example.com");
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", EncodeToBase64($"{userName}:{"password"}"));

            var actionDescriptor = new Mock<HttpActionDescriptor>();

            var controllerContext = new HttpControllerContext(context.Object, new HttpRouteData(new HttpRoute()), requestMessage);
            var actionContext = new HttpActionContext(controllerContext, actionDescriptor.Object);

            Func<string, string, bool> validateUserCredentials = (s, s1) => true;

            var attribute = CreateApiBasicAuthAttribute(validateUserCredentials, true);

            attribute.OnAuthorization(actionContext);

            Assert.AreEqual(userName, Thread.CurrentPrincipal.Identity.Name);
        }

        private static string userName = "name";
        private static string userEmail = "user@mail";
        private static readonly Mock<DesignerMembershipUser> membershipUserMock = new Mock<DesignerMembershipUser>();
    }
}
