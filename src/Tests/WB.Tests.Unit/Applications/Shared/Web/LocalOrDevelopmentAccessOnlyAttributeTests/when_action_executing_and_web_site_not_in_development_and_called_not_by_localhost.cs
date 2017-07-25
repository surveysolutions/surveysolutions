using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.Mvc;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.UI.Shared.Web.Configuration;
using WB.UI.Shared.Web.Filters;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Shared.Web.LocalOrDevelopmentAccessOnlyAttributeTests
{
    internal class when_action_executing_and_web_site_not_in_development_and_called_not_by_localhost : LocalOrDevelopmentAccessOnlyAttributeTestsContext
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

        It should_exception_not_be_null = () =>
            exception.ShouldNotBeNull();

        It should_exception_exact_of_type_HttpException = () =>
             exception.ShouldBeOfExactType<HttpException>();

        It should_http_code_in_http_exception__be_equal_to_404 = () =>
            ((HttpException)exception).GetHttpCode().ShouldEqual(403);
        
        private static LocalOrDevelopmentAccessOnlyAttribute filter;
        private static ActionExecutingContext actionExecutingContext = CreateFilterContext(IsLocalhost);
        private static Exception exception;
        private static bool IsWebsiteUnderDevelopment = false;
        private static bool IsLocalhost = false;
    }
}