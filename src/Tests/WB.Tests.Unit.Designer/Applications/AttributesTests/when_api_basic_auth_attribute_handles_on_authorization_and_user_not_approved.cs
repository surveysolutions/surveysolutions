using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using System.Web.Security;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership;
using WB.Core.BoundedContexts.Designer.Services.Accounts;
using WB.UI.Designer.Api.Attributes;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;

namespace WB.Tests.Unit.Designer.Applications.AttributesTests
{
    internal class when_api_basic_auth_attribute_handles_on_authorization_and_user_not_approved : AttributesTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var membershipUserServiceMock = new Mock<IMembershipUserService>();
            var membershipWebUserMock = new Mock<IMembershipWebUser>();
            var membershipUserMock = new Mock<DesignerMembershipUser>();
            membershipUserMock.Setup(x => x.IsApproved).Returns(false);
            membershipUserMock.Setup(x => x.Email).Returns(userEmail);
            membershipWebUserMock.Setup(x => x.MembershipUser).Returns(membershipUserMock.Object);
            membershipUserServiceMock.Setup(_ => _.WebUser).Returns(membershipWebUserMock.Object);

            Mock.Get(ServiceLocator.Current).Setup(_ => _.GetInstance<IMembershipUserService>()).Returns(membershipUserServiceMock.Object);
            Setup.InstanceToMockedServiceLocator<IAccountRepository>(
                Mock.Of<IAccountRepository>(x => x.GetByNameOrEmail(userName) == Mock.Of<IMembershipAccount>(a => a.UserName == userName)));

            var context = new Mock<HttpConfiguration>();

            var requestMessage = new HttpRequestMessage();
            requestMessage.RequestUri = new Uri("http://www.example.com");

            requestMessage.Headers.Authorization = new AuthenticationHeaderValue(
                "Basic", EncodeToBase64(string.Format("{0}:{1}", userName, "password")));

            var actionDescriptor = new Mock<HttpActionDescriptor>();

            var controllerContext = new HttpControllerContext(context.Object, new HttpRouteData(new HttpRoute()), requestMessage);
            filterContext = new HttpActionContext(controllerContext, actionDescriptor.Object);

            Func<string, string, bool> validateUserCredentials = (s, s1) => true;

            attribute = CreateApiBasicAuthAttribute(validateUserCredentials);
            BecauseOf();
        }

        private void BecauseOf() =>
            attribute.OnAuthorization(filterContext);
        
        [NUnit.Framework.Test] public void should_response_context_contains_unauthorized_exception () =>
            filterContext.Response.StatusCode.ShouldEqual(HttpStatusCode.Unauthorized);

        [NUnit.Framework.Test] public void should_response_context_unauthorized_exception_has_specified_reasonphrase () =>
            filterContext.Response.ReasonPhrase.ShouldEqual(expectedReasonPhrase);

        private static ApiBasicAuthAttribute attribute;
        private static HttpActionContext filterContext;

        private static string userName = "name";
        private static string userEmail = "user@mail";

        private static readonly string expectedReasonPhrase =
            string.Format("Your account is not approved. Please, confirm your account first. We've sent a confirmation link to {0}.", userEmail);
    }
}
