using System.Collections.Generic;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Filters;

namespace WB.UI.Headquarters.Tests.FilterTests.InstallationAttributeTests
{
    internal class InstallationAttributeTestsContext
    {
        protected static InstallationAttribute Create(IIdentityManager identityManager = null)
        {
            var serviceLocatorMock = new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock };
            ServiceLocator.SetLocatorProvider(() => serviceLocatorMock.Object);

            serviceLocatorMock.Setup(_ => _.GetInstance<IIdentityManager>())
                .Returns(identityManager ?? Mock.Of<IIdentityManager>());

            return new InstallationAttribute();
        }

        protected static ActionExecutingContext CreateFilterContext(bool isUserAuthenticated,
            ControllerBase specifiedController = null)
        {
            var identityMock = new Mock<IIdentity>();
            identityMock.Setup(_ => _.IsAuthenticated).Returns(isUserAuthenticated);

            var principalMock = new Mock<IPrincipal>();
            principalMock.Setup(_ => _.Identity).Returns(identityMock.Object);

            var httpContext = new Mock<HttpContextBase>();
            httpContext.Setup(_ => _.User).Returns(principalMock.Object);

            var requestContext = new RequestContext(httpContext.Object, new RouteData());

            var controllerContext = new ControllerContext(requestContext,
                specifiedController ?? Mock.Of<ControllerBase>());

            return new ActionExecutingContext(controllerContext, new Mock<ActionDescriptor>().Object,
                new Dictionary<string, object>());
        }
    }
}