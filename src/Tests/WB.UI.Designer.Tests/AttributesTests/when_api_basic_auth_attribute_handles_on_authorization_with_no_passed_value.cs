using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using System.Web.Mvc;
using System.Web.Routing;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Group;
using WB.UI.Designer.Api.Attributes;
using It = Machine.Specifications.It;

namespace WB.UI.Designer.Tests.AttributesTests
{
    public class when_api_basic_auth_attribute_handles_on_authorization_with_no_passed_value : AttributesTestContext
    {
        Establish context = () =>
        {
            var context = new Mock<HttpConfiguration>();
            
            var requestMessage = new HttpRequestMessage();
            requestMessage.RequestUri = new Uri("http://www.example.com");
            
            var actionDescriptor = new Mock<HttpActionDescriptor>();
            
            var controllerContext = new HttpControllerContext(context.Object, new HttpRouteData(new HttpRoute()), requestMessage);
            filterContext = new HttpActionContext(controllerContext, actionDescriptor.Object);

            attribute = CreateApiBasicAuthAttribute();
        };

        Because of = () =>
            attribute.OnAuthorization(filterContext);

        It should_return_unauthorized_status_code = () =>
            filterContext.Response.StatusCode.ShouldEqual(HttpStatusCode.Unauthorized);

        private static ApiBasicAuthAttribute attribute;
        private static HttpActionContext filterContext;
    }
}
