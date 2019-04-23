using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.MembershipProvider;

namespace WB.Tests.Unit.Designer.Applications.AttributesTests
{
    internal class when_api_basic_auth_attribute_handles_on_authorization_and_user_not_approved : AttributesTestContext
    {
        [Test]
        public async Task context ()
        {
            var userMock = new Mock<DesignerIdentityUser>();
            userMock.Setup(s => s.UserName).Returns(userName);
            userMock.Setup(s => s.EmailConfirmed).Returns(false);
            userMock.Setup(s => s.Email).Returns(userEmail);

            var userStore = CreateAndSetupUserStore(userMock.Object);
            var filterContext = CreateAndSetupActionFilterContext(userName); 
            var filter = CreateApiBasicAuthFilter(userStore);


            await filter.OnAuthorizationAsync(filterContext);


            // should_response_context_contains_unauthorized_exception
            Assert.AreEqual(StatusCodes.Status401Unauthorized, ((ContentResult)filterContext.Result).StatusCode);
            // should_response_context_unauthorized_exception_has_specified_reasonphrase
            Assert.AreEqual(expectedReasonPhrase, ((ContentResult)filterContext.Result).Content);
        }

        private static string userName = "name";
        private static string userEmail = "user@mail";

        private static readonly string expectedReasonPhrase =
            $"Your account is not approved. Please, confirm your account first. We've sent a confirmation link to {userEmail}.";
    }
}
