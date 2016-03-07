using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;
using NSubstitute;
using WB.UI.Headquarters.Filters;

namespace WB.Tests.Unit.Applications.Headquarters.FilterTests.SupervisorFunctionsEnabledAttributeTests
{
    internal class SupervisorFunctionsEnabledAttributeTestsContext
    {
        protected static SupervisorFunctionsEnabledAttribute Create()
        {
            return new SupervisorFunctionsEnabledAttribute();
        }

        protected static HttpActionContext CreateFilterContext(IHttpController specifiedApiController = null)
        {
            return new HttpActionContext(
                new HttpControllerContext(
                    new HttpRequestContext(),
                    new HttpRequestMessage(),
                    new HttpControllerDescriptor(),
                    specifiedApiController),
                Substitute.For<HttpActionDescriptor>());
        }
    }
}