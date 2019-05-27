using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Applications.Headquarters.UtilsTests
{
    public class CookieRedirectTests
    {
        [SetUp]
        public void Setup()
        {
            RequestHeaders = new HeaderDictionary(new Dictionary<string, string[]>());
            ResponseHeaders = new HeaderDictionary(new Dictionary<string, string[]>());

            var request = new Mock<IOwinRequest>();
            request.Setup(r => r.IsSecure).Returns(() => isSecure);
            request.Setup(r => r.Headers).Returns(() => RequestHeaders);
            var response = new Mock<IOwinResponse>();
            response.Setup(r => r.Headers).Returns(() => ResponseHeaders);
            response.Setup(r => r.Redirect(It.IsAny<string>())).Callback<string>(uri => RedirectedUri = uri);

            owin = Mock.Of<IOwinContext>(c => c.Request == request.Object && c.Response == response.Object);
        }

        IHeaderDictionary RequestHeaders;
        IHeaderDictionary ResponseHeaders;
        bool isSecure;

        private IOwinContext owin;
        private string RedirectedUri = null;

        [TestCase("HTtp://example.com", ExpectedResult = "https://example.com")]
        [TestCase("http://example.com/http://sdfsdfbc ", ExpectedResult = "https://example.com/http://sdfsdfbc ")]
        public string ShouldApplyProperRedirectToHttpsInCaseOfSecureConnection(string redirectUri)
        {
            isSecure = true;
            var ctx = new CookieApplyRedirectContext(owin, null, redirectUri);

            RedirectHelper.ApplyNonApiRedirect(ctx);

            return RedirectedUri;
        }

        [TestCase("https", true)]
        [TestCase("http", false)]
        public void Should_Apply_Redirect_When_X_Forwarded_Proto_present(string proto, bool isRedirectApplied)
        {
            isSecure = false;
            RequestHeaders.Add("X-Forwarded-Proto", new[] { proto });
            var ctx = new CookieApplyRedirectContext(owin, null, "http://example.com");

            RedirectHelper.ApplyNonApiRedirect(ctx);

            Assert.That(RedirectedUri, Is.EqualTo(isRedirectApplied ? "https://example.com" : "http://example.com"));
        }
    }
}
