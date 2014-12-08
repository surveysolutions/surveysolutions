﻿using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.Infrastructure.ReadSide;
using WB.UI.Designer.Filters;

namespace WB.Tests.Unit.Applications.Designer.FiltersTests.MaintenanceFilterTests
{
    internal class MaintenanceFilterTestsContext
    {
        protected static MaintenanceFilter Create(IReadSideStatusService readSideStatusService = null)
        {
            Setup.InstanceToMockedServiceLocator<IReadSideStatusService>(readSideStatusService ?? Mock.Of<IReadSideStatusService>());

            return new MaintenanceFilter();
        }

        protected static ActionExecutingContext CreateFilterContext(ControllerBase specifiedController = null, string returnUrl = null)
        {
            var httpContextBaseMock = new Mock<HttpContextBase>();
            var httpRequestBaseMock = new Mock<HttpRequestBase>();
            
            httpContextBaseMock.Setup(_ => _.Request).Returns(httpRequestBaseMock.Object);
            httpRequestBaseMock.Setup(_ => _.Url).Returns(new Uri(returnUrl ?? "/"));

            var requestContext = new RequestContext(httpContextBaseMock.Object, new RouteData());

            var controllerContext = new ControllerContext(requestContext,
                specifiedController ?? Mock.Of<ControllerBase>());

            return new ActionExecutingContext(controllerContext, new Mock<ActionDescriptor>().Object,
                new Dictionary<string, object>());
        }
    }
}