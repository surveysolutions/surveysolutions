using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.UI.Designer.Api.Attributes;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Designer.AttributesTests
{
    public class when_api_basic_auth_attribute_handles_on_authorization_with_incorrect_credentials : AttributesTestContext
    {
        Establish context = () =>
        {
            var userName = "name";

            var context = new Mock<HttpConfiguration>();

            var requestMessage = new HttpRequestMessage();
            requestMessage.RequestUri = new Uri("http://www.example.com");

            requestMessage.Headers.Authorization = new AuthenticationHeaderValue(
                "Basic", EncodeToBase64(string.Format("{0}:{1}", userName, "password")));


            var actionDescriptor = new Mock<HttpActionDescriptor>();

            var controllerContext = new HttpControllerContext(context.Object, new HttpRouteData(new HttpRoute()), requestMessage);
            filterContext = new HttpActionContext(controllerContext, actionDescriptor.Object);

            Func<string, string, bool> validateUserCredentials = (s, s1) => false;

            attribute = CreateApiBasicAuthAttribute(validateUserCredentials);
        };

        Because of = () =>
            attribute.OnAuthorization(filterContext);

        It should_return_unauthorized_status_code = () =>
            filterContext.Response.StatusCode.ShouldEqual(HttpStatusCode.Unauthorized);

        private static ApiBasicAuthAttribute attribute;
        private static HttpActionContext filterContext;
    }
}
