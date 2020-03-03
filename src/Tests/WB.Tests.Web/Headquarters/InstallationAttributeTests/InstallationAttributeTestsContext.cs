using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using Amazon.Runtime.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Tests.Abc;

using WB.UI.Headquarters.Filters;

namespace WB.Tests.Unit.Applications.Headquarters.FilterTests.InstallationAttributeTests
{
    internal class InstallationAttributeTestsContext
    {
        protected static InstallationFilter CreateInstallationAttribute()
        {
            var installationAttribute = new InstallationFilter();
            InstallationFilter.Installed = false;
            return installationAttribute;
        }

        protected static ActionExecutingContext CreateFilterContext(ControllerBase specifiedController = null, IUserRepository userRepository = null)
        {
            var defaultHttpContext = new DefaultHttpContext();

            var serviceContainer = new ServiceContainer();
            serviceContainer.AddService(typeof(IUserRepository), userRepository ?? Create.Storage.UserRepository());
            defaultHttpContext.RequestServices = serviceContainer;

            return new ActionExecutingContext(new ActionContext
                {
                    HttpContext = defaultHttpContext,
                    RouteData = new RouteData(),
                    ActionDescriptor = new ActionDescriptor()
                }, 
                new List<IFilterMetadata>(), 
                new Dictionary<string, object>(), 
                specifiedController);
        }
    }
}
