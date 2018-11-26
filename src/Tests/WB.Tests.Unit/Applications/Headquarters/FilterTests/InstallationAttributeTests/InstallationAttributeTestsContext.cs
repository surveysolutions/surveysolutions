using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;
using NSubstitute;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Tests.Abc;
using WB.UI.Headquarters.Filters;

namespace WB.Tests.Unit.Applications.Headquarters.FilterTests.InstallationAttributeTests
{
    internal class InstallationAttributeTestsContext
    {
        protected static InstallationAttribute CreateInstallationAttribute()
        {
            var installationAttribute = new InstallationAttribute();
            InstallationAttribute.Installed = false;
            return installationAttribute;
        }

        protected static ActionExecutingContext CreateFilterContext(ControllerBase specifiedController = null, IUserRepository userRepository = null)
        {
            var requestContext = new RequestContext(Mock.Of<HttpContextBase>(), new RouteData());

            var controllerContext = new ControllerContext(requestContext,
                specifiedController ?? Mock.Of<ControllerBase>());

            Setup.InstanceToMockedServiceLocator(userRepository ?? Create.Storage.UserRepository());
            var users = userRepository ?? Create.Storage.UserRepository();
            
            var dependencyResolver = Substitute.For<IDependencyResolver>();
            dependencyResolver
                .GetService<IUserRepository>()
                .Returns(users);

            DependencyResolver.SetResolver(dependencyResolver);

            return new ActionExecutingContext(controllerContext, new Mock<ActionDescriptor>().Object,
                new Dictionary<string, object>());
        }
    }
}
