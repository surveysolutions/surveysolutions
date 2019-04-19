using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;


namespace WB.Tests.Unit.Designer.Applications.AttributesTests
{
    internal class when_api_basic_auth_attribute_handles_on_authorization_with_incorrect_credentials : AttributesTestContext
    {
        [Test]
        public async Task context()
        {
            var userName = "name";

            var userStore = CreateAndSetupEmptyUserStore();
            var filterContext = CreateAndSetupActionFilterContext(userName);
            var filter = CreateApiBasicAuthFilter(userStore);

            // Act
            await filter.OnAuthorizationAsync(filterContext);

            // should_return_unauthorized_status_code () =>
            Assert.AreEqual(StatusCodes.Status401Unauthorized, ((ContentResult)filterContext.Result).StatusCode);
        }
    }
}
