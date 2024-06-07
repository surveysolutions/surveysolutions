using System.Net;
using Moq;
using NUnit.Framework;

namespace WB.Tests.Unit.Designer.Api
{
    // TODO: KP-13523 Fix HqUserManager related tests
    //[TestFixture]
    //public class CustomWebApiAuthorizeFilterTests
    //{
    //    [Test]
    //    public void OnAuthorization_when_call_api_when_user_was_locked_should_return_unauthorized()
    //    {
    //        var membershipUserService = Mock.Of<IMembershipUserService> (s => s.WebUser.MembershipUser.IsLockedOut == true);
    //        Setup.ServiceLocatorForCustomWebApiAuthorizeFilter(membershipUserService: membershipUserService);
    //        Setup.HttpContextWithIsAuthenticatedFlag();
    //        CustomWebApiAuthorizeFilter filter = Create.CustomWebApiAuthorizeFilter();
    //        var context = Setup.HttpActionContextWithOutAllowAnonymousOnAction();

    //        filter.OnAuthorization(context);

    //        ClassicAssert.AreEqual(context.Response.StatusCode, HttpStatusCode.Unauthorized);
    //        Mock.Get(membershipUserService).Verify(x => x.Logout(), Times.Once);
    //    }


    //    [Test]
    //    public void OnAuthorization_when_call_api_when_user_not_approved_should_return_unauthorized()
    //    {
    //        var membershipUserService = Mock.Of<IMembershipUserService> (s => s.WebUser.MembershipUser.IsApproved == false);
    //        Setup.ServiceLocatorForCustomWebApiAuthorizeFilter(membershipUserService: membershipUserService);
    //        Setup.HttpContextWithIsAuthenticatedFlag();
    //        CustomWebApiAuthorizeFilter filter = Create.CustomWebApiAuthorizeFilter();
    //        var context = Setup.HttpActionContextWithOutAllowAnonymousOnAction();

    //        filter.OnAuthorization(context);

    //        ClassicAssert.AreEqual(context.Response.StatusCode, HttpStatusCode.Unauthorized);
    //        Mock.Get(membershipUserService).Verify(x => x.Logout(), Times.Once);
    //    }


    //    [Test]
    //    public void OnAuthorization_when_call_api_when_user_without_restriction_should_allow_user()
    //    {
    //        var membershipUserService = Mock.Of<IMembershipUserService> (s => s.WebUser.MembershipUser.IsApproved == true);
    //        Setup.ServiceLocatorForCustomWebApiAuthorizeFilter(membershipUserService: membershipUserService);
    //        Setup.HttpContextWithIsAuthenticatedFlag();
    //        CustomWebApiAuthorizeFilter filter = Create.CustomWebApiAuthorizeFilter();
    //        var context = Setup.HttpActionContextWithOutAllowAnonymousOnAction();

    //        filter.OnAuthorization(context);

    //        ClassicAssert.Null(context.Response);
    //    }
    //}
}
