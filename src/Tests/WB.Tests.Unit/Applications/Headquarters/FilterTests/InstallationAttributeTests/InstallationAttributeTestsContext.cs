using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Tests.Abc;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Filters;

namespace WB.Tests.Unit.Applications.Headquarters.FilterTests.InstallationAttributeTests
{
    internal class InstallationAttributeTestsContext
    {
        protected static InstallationAttribute Create(IIdentityManager identityManager = null)
        {
            Setup.InstanceToMockedServiceLocator(identityManager ?? Mock.Of<IIdentityManager>());

            var installationAttribute = new InstallationAttribute();
            InstallationAttribute.Installed = false;
            return installationAttribute;
        }

        protected static ActionExecutingContext CreateFilterContext(ControllerBase specifiedController = null)
        {
            var requestContext = new RequestContext(Mock.Of<HttpContextBase>(), new RouteData());

            var controllerContext = new ControllerContext(requestContext,
                specifiedController ?? Mock.Of<ControllerBase>());

            return new ActionExecutingContext(controllerContext, new Mock<ActionDescriptor>().Object,
                new Dictionary<string, object>());
        }
    }
}