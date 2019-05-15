using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Services;
using WB.UI.Designer.Resources;

namespace WB.Tests.Unit.Designer.Applications.AttributesTests
{
    internal class when_api_basic_auth_attribute_handles_on_authorization_with_IP_not_in_list : AttributesTestContext
    {
        [Test]
        public async Task context()
        {
            var userName = "name";

            var userMock = new Mock<DesignerIdentityUser>();
            userMock.Setup(s => s.UserName).Returns(userName);
            userMock.Setup(s => s.Email).Returns("user@user.com");
            userMock.Setup(s => s.EmailConfirmed).Returns(true);
            userMock.Setup(s => s.LockoutEnabled).Returns(false);

            string IPAddressToChecks = "1.2.3.4";
            var address = IPAddress.Parse(IPAddressToChecks);
            var allowedAddressService = Mock.Of<IAllowedAddressService>(x => x.IsAllowedAddress(address) == false);
            var ipAddressProvider = Mock.Of<IIpAddressProvider>(x => x.GetClientIpAddress() == address);

            var userStore = CreateAndSetupUserStore(userMock.Object);
            var filterContext = CreateAndSetupActionFilterContext(userName);
            var filter = CreateApiBasicAuthFilter(userStore, ipAddressProvider, allowedAddressService, onlyAllowedAddresses: true);
            

            // Act
            await filter.OnAuthorizationAsync(filterContext);

            // should_return_forbidden_status_code() =>
            Assert.AreEqual(StatusCodes.Status403Forbidden, ((ContentResult)filterContext.Result).StatusCode);
            // should_return_message_containing_IP() =>
            Assert.AreEqual(string.Format(ErrorMessages.UserNeedToContactSupportFormat, IPAddressToChecks), ((ContentResult)filterContext.Result).Content);
        }
    }
}
