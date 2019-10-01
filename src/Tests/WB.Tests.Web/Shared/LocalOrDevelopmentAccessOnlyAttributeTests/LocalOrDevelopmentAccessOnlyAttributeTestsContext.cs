using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;
using WB.UI.Shared.Web.Filters;

namespace WB.Tests.Unit.Applications.Shared.Web.LocalOrDevelopmentAccessOnlyAttributeTests
{
    internal class LocalOrDevelopmentAccessOnlyAttributeTestsContext
    {
        protected static LocalOrDevelopmentAccessOnlyAttribute Create()
        {
            return new LocalOrDevelopmentAccessOnlyAttribute();
        }

        protected static ActionExecutingContext CreateFilterContext(bool isLocal)
        {
            var httpContextBaseMock = new Mock<HttpContextBase>();
            var httpRequestBaseMock = new Mock<HttpRequestBase>();
            
            httpContextBaseMock.Setup(_ => _.Request).Returns(httpRequestBaseMock.Object);
            httpRequestBaseMock.Setup(_ => _.IsLocal).Returns(isLocal);

            var requestContext = new RequestContext(httpContextBaseMock.Object, new RouteData());

            var controllerContext = new ControllerContext(requestContext, Mock.Of<ControllerBase>());

            return new ActionExecutingContext(controllerContext, new Mock<ActionDescriptor>().Object,
                new Dictionary<string, object>());
        }
    }
}