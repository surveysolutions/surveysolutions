using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.MembershipProvider;

namespace WB.Tests.Unit.Designer.Applications.AttributesTests
{
    internal class when_api_basic_auth_attribute_handles_on_authorization_with_correct_credentials : AttributesTestContext
    {
        [Test]
        public async Task context ()
        {
            var userMock = new Mock<DesignerIdentityUser>();
            userMock.Setup(s => s.UserName).Returns(userName);
            userMock.Setup(s => s.Email).Returns("user@user.com");
            userMock.Setup(s => s.EmailConfirmed).Returns(true);
            userMock.Setup(s => s.LockoutEnabled).Returns(false);

            var userStore = CreateAndSetupUserStore(userMock.Object);
            var filterContext = CreateAndSetupActionFilterContext(userName); 
            var filter = CreateApiBasicAuthFilter(userStore);

            //Act
            await filter.OnAuthorizationAsync(filterContext);

            // should_set_Thread_Identity_name_to_proveded_value() => 
            Assert.That(filterContext.HttpContext.User.Identity.Name, Is.EqualTo(this.userName));
            // should_set_Thread_Identity_IsAuthenticated_to_true() =>
            Assert.That(filterContext.HttpContext.User.Identity.IsAuthenticated, Is.True);
        }

        string userName = "name";
    }
}
