using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using Machine.Specifications;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Services.Accounts;
using WB.UI.Designer.Api.Attributes;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;

namespace WB.Tests.Unit.Designer.Applications.AttributesTests
{
    internal class when_api_basic_auth_attribute_handles_on_authorization_with_IP_not_in_list : AttributesTestContext
    {
        [OneTimeSetUp]
        public void context()
        {
            AssemblyContext.SetupServiceLocator();

            var membershipUserServiceMock = new Mock<IMembershipUserService>();
            var membershipWebUserMock = new Mock<IMembershipWebUser>();
            var membershipUserMock = new Mock<DesignerMembershipUser>();
            membershipUserMock.Setup(x => x.IsApproved).Returns(true);
            membershipUserMock.Setup(x => x.IsLockedOut).Returns(false);
            membershipWebUserMock.Setup(x => x.MembershipUser).Returns(membershipUserMock.Object);
            membershipUserServiceMock.Setup(_ => _.WebUser).Returns(membershipWebUserMock.Object);
            
            Mock.Get(ServiceLocator.Current).Setup(_ => _.GetInstance<IMembershipUserService>()).Returns(membershipUserServiceMock.Object);
            Setup.InstanceToMockedServiceLocator<IAccountRepository>(
                Mock.Of<IAccountRepository>(x => x.GetByNameOrEmail(userName) == Mock.Of<IMembershipAccount>(a => a.UserName == userName)));

            var address = IPAddress.Any;

            Setup.InstanceToMockedServiceLocator<IIpAddressProvider>(
                Mock.Of<IIpAddressProvider>(x => x.GetClientIpAddress() == address));

            Setup.InstanceToMockedServiceLocator<IAllowedAddressService>(
                Mock.Of<IAllowedAddressService>(x => x.IsAllowedAddress(address) == false));
            
            var context = new Mock<HttpConfiguration>();

            var requestMessage = new HttpRequestMessage();
            requestMessage.RequestUri = new Uri("http://www.example.com");

            requestMessage.Headers.Authorization = new AuthenticationHeaderValue(
                "Basic", EncodeToBase64(string.Format("{0}:{1}", userName, "password")));

            var actionDescriptor = new Mock<HttpActionDescriptor>();

            var controllerContext = new HttpControllerContext(context.Object, new HttpRouteData(new HttpRoute()), requestMessage);
            filterContext = new HttpActionContext(controllerContext, actionDescriptor.Object);

            Func<string, string, bool> validateUserCredentials = (s, s1) => true;

            attribute = CreateApiBasicAuthAttribute(validateUserCredentials, true);

            //Act
            attribute.OnAuthorization(filterContext);
        }
        
        
        [NUnit.Framework.Test]
        public void should_return_forbidden_status_code() =>
            filterContext.Response.StatusCode.ShouldEqual(HttpStatusCode.Forbidden);


        [NUnit.Framework.Test]
        public void should_return_message_containing_IP() =>
            filterContext.Response.ReasonPhrase.ShouldContain("IP:0.0.0.0");


        ApiBasicAuthAttribute attribute;
        HttpActionContext filterContext;

        string userName = "name";
    }
}
