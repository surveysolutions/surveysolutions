using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.Mvc;
using FluentAssertions;
using Moq;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.UI.Shared.Web.Configuration;
using WB.UI.Shared.Web.Filters;


namespace WB.Tests.Unit.Applications.Shared.Web.LocalOrDevelopmentAccessOnlyAttributeTests
{
    internal class when_action_executing_and_web_site_not_in_development_and_called_not_by_localhost : LocalOrDevelopmentAccessOnlyAttributeTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var configMock =new Mock<IConfigurationManager>();
            configMock.Setup(_ => _.AppSettings).Returns(new NameValueCollection { { "IsDevelopmentEnvironment", IsWebsiteUnderDevelopment.ToString() } });
            Mock.Get(ServiceLocator.Current).Setup(_ => _.GetInstance<IConfigurationManager>()).Returns(configMock.Object);

            filter = Create();
            BecauseOf();
        }

        public void BecauseOf() =>
            exception = NUnit.Framework.Assert.Throws<HttpException>(() => filter.OnActionExecuting(actionExecutingContext));

        [NUnit.Framework.Test] public void should_exception_not_be_null () =>
            exception.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_exception_exact_of_type_HttpException () =>
             exception.Should().BeOfType<HttpException>();

        [NUnit.Framework.Test] public void should_http_code_in_http_exception__be_equal_to_404 () =>
            ((HttpException)exception).GetHttpCode().Should().Be(403);
        
        private static LocalOrDevelopmentAccessOnlyAttribute filter;
        private static ActionExecutingContext actionExecutingContext = CreateFilterContext(IsLocalhost);
        private static Exception exception;
        private static bool IsWebsiteUnderDevelopment = false;
        private static bool IsLocalhost = false;
    }
}
