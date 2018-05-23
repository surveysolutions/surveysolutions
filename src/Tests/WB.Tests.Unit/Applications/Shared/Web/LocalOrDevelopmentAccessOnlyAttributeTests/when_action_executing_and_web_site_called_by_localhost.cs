using System.Collections.Specialized;
using System.Web.Mvc;
using Moq;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.UI.Shared.Web.Configuration;
using WB.UI.Shared.Web.Filters;


namespace WB.Tests.Unit.Applications.Shared.Web.LocalOrDevelopmentAccessOnlyAttributeTests
{
    internal class when_action_executing_and_web_site_called_by_localhost : LocalOrDevelopmentAccessOnlyAttributeTestsContext
    {
        [NUnit.Framework.Test] public void context () {
            var configMock =new Mock<IConfigurationManager>();
            configMock.Setup(_ => _.AppSettings).Returns(new NameValueCollection { { "IsDevelopmentEnvironment", IsWebsiteUnderDevelopment.ToString() } });
            Mock.Get(ServiceLocator.Current).Setup(_ => _.GetInstance<IConfigurationManager>()).Returns(configMock.Object);

            filter = Create();
            filter.OnActionExecuting(actionExecutingContext);
        }

        private static LocalOrDevelopmentAccessOnlyAttribute filter;
        private static bool IsWebsiteUnderDevelopment = false;
        private static bool IsLocalhost = true;
        private static ActionExecutingContext actionExecutingContext = CreateFilterContext(IsLocalhost);
    }
}
