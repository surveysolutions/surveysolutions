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
        public void OnAuthorization_when_call_api_when_rebuild_readside_executed_should_return_forbidden_status_with_json_content()
        {
            var readSideStatusService = Mock.Of<IReadSideStatusService>(s => s.AreViewsBeingRebuiltNow() == true);
            Setup.ServiceLocatorForCustomWebApiAuthorizeFilter(readSideStatusService: readSideStatusService);
            CustomWebApiAuthorizeFilter filter = Create.CustomWebApiAuthorizeFilter();
            var context = Setup.HttpActionContextWithOutAllowAnonymousOnAction();
            
            filter.OnAuthorization(context);

            Assert.AreEqual(context.Response.StatusCode, HttpStatusCode.Forbidden);
            Assert.AreEqual(context.Response.Content.Headers.ContentType.MediaType, "application/json");
            Assert.AreEqual(context.Response.Content.Headers.ContentType.CharSet, "utf-8");
        }


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