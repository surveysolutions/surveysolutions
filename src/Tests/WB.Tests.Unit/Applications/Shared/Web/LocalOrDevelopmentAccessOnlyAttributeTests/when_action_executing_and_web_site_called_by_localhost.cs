﻿using System;
using System.Collections.Specialized;
using System.Web.Mvc;
using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.UI.Shared.Web.Configuration;
using WB.UI.Shared.Web.Filters;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Shared.Web.LocalOrDevelopmentAccessOnlyAttributeTests
{
    internal class when_action_executing_and_web_site_called_by_localhost : LocalOrDevelopmentAccessOnlyAttributeTestsContext
    {
        Establish context = () =>
        {
            var configMock =new Mock<IConfigurationManager>();
            configMock.Setup(_ => _.AppSettings).Returns(new NameValueCollection { { "IsDevelopmentEnvironment", IsWebsiteUnderDevelopment.ToString() } });
            Mock.Get(ServiceLocator.Current).Setup(_ => _.GetInstance<IConfigurationManager>()).Returns(configMock.Object);

            filter = Create();
        };

        Because of = () =>
            exception = Catch.Exception(() => filter.OnActionExecuting(actionExecutingContext));

        It should_exception_not_null = () =>
            exception.ShouldBeNull();
        
        private static LocalOrDevelopmentAccessOnlyAttribute filter;
        private static Exception exception;
        private static bool IsWebsiteUnderDevelopment = false;
        private static bool IsLocalhost = true;
        private static ActionExecutingContext actionExecutingContext = CreateFilterContext(IsLocalhost);
    }
}