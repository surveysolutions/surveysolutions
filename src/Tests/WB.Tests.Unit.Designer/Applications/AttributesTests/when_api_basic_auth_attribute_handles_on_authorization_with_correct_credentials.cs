using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using System.Web.Security;
using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.UI.Designer.Api.Attributes;
using WB.UI.Shared.Web.Membership;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.Applications.AttributesTests
{
    internal class when_api_basic_auth_attribute_handles_on_authorization_with_correct_credentials : AttributesTestContext
    {
        Establish context = () =>
        {
            var membershipUserServiceMock = new Mock<IMembershipUserService>();
            var membershipWebUserMock = new Mock<IMembershipWebUser>();
            var membershipUserMock = new Mock<MembershipUser>();
            membershipUserMock.Setup(x => x.IsApproved).Returns(true);
            membershipUserMock.Setup(x => x.IsLockedOut).Returns(false);
            membershipWebUserMock.Setup(x => x.MembershipUser).Returns(membershipUserMock.Object);
            membershipUserServiceMock.Setup(_ => _.WebUser).Returns(membershipWebUserMock.Object);

            Mock.Get(ServiceLocator.Current).Setup(_ => _.GetInstance<IMembershipUserService>()).Returns(membershipUserServiceMock.Object);

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
        };

        Because of = () =>
            attribute.OnAuthorization(filterContext);

        It should_set_Thread_Identity_name_to_proveded_value = () =>
            Thread.CurrentPrincipal.Identity.Name.ShouldEqual(userName);

        It should_set_Thread_Identity_IsAuthenticated_to_true = () =>
            Thread.CurrentPrincipal.Identity.IsAuthenticated.ShouldBeTrue();

        private static ApiBasicAuthAttribute attribute;
        private static HttpActionContext filterContext;

        private static string userName = "name";
    }
}
