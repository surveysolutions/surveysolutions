using System.Net;
using Moq;
using NUnit.Framework;
using WB.Core.Infrastructure.ReadSide;
using WB.UI.Designer.Code;
using WB.UI.Shared.Web.Membership;

namespace WB.Tests.Unit.Designer.Api
{
    [TestFixture]
    public class CustomWebApiAuthorizeFilterTests
    {
        [Test]
        public void OnAuthorization_when_call_api_when_user_was_locked_should_return_unauthorized()
        {
            var membershipUserService = Mock.Of<IMembershipUserService> (s => s.WebUser.MembershipUser.IsLockedOut == true);
            Setup.ServiceLocatorForCustomWebApiAuthorizeFilter(membershipUserService: membershipUserService);
            Setup.HttpContextWithIsAuthenticatedFlag();
            CustomWebApiAuthorizeFilter filter = Create.CustomWebApiAuthorizeFilter();
            var context = Setup.HttpActionContextWithOutAllowAnonymousOnAction();
            
            filter.OnAuthorization(context);

            Assert.AreEqual(context.Response.StatusCode, HttpStatusCode.Unauthorized);
            Mock.Get(membershipUserService).Verify(x => x.Logout(), Times.Once);
        }


        [Test]
        public void OnAuthorization_when_call_api_when_user_not_approved_should_return_unauthorized()
        {
            var membershipUserService = Mock.Of<IMembershipUserService> (s => s.WebUser.MembershipUser.IsApproved == false);
            Setup.ServiceLocatorForCustomWebApiAuthorizeFilter(membershipUserService: membershipUserService);
            Setup.HttpContextWithIsAuthenticatedFlag();
            CustomWebApiAuthorizeFilter filter = Create.CustomWebApiAuthorizeFilter();
            var context = Setup.HttpActionContextWithOutAllowAnonymousOnAction();
            
            filter.OnAuthorization(context);

            Assert.AreEqual(context.Response.StatusCode, HttpStatusCode.Unauthorized);
            Mock.Get(membershipUserService).Verify(x => x.Logout(), Times.Once);
        }


        [Test]
        public void OnAuthorization_when_call_api_when_user_without_restriction_should_allow_user()
        {
            var membershipUserService = Mock.Of<IMembershipUserService> (s => s.WebUser.MembershipUser.IsApproved == true);
            Setup.ServiceLocatorForCustomWebApiAuthorizeFilter(membershipUserService: membershipUserService);
            Setup.HttpContextWithIsAuthenticatedFlag();
            CustomWebApiAuthorizeFilter filter = Create.CustomWebApiAuthorizeFilter();
            var context = Setup.HttpActionContextWithOutAllowAnonymousOnAction();
            
            filter.OnAuthorization(context);

            Assert.Null(context.Response);
        }
    }
}