using System;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;

namespace WB.Tests.Unit
{
    [TestFixture]
    public class ExtensionsTests
    {
        [TestCase(20, 4, null, "org.worldbank.solutions.interviewer/20.04 (build 27410) (QuestionnaireVersion/29.0.0)")]
        [TestCase(20, 4, 7, "org.worldbank.solutions.interviewer/20.04.7 (build 27417) (QuestionnaireVersion/29.0.0)")]
        [TestCase(20, 4, 7, "org.worldbank.solutions.supervisor/20.04.7 (build 27417) (QuestionnaireVersion/29.0.0)")]
        [TestCase(20, 4, 7, "org.worldbank.solutions.interviewer/20.04.7 (build 25532) (DEBUG QuestionnaireVersion/29.0.0)")]
        public void when_get_user_agent_with_version(int major, int minor, int? build, string userAgent)
        {
            IHeaderDictionary headers = new HeaderDictionary();
            headers.Append("User-Agent", userAgent);
            var httpRequest = Mock.Of<HttpRequest>(r => r.Headers == headers);
            var version = WB.UI.Headquarters.Code.Extensions.GetProductVersionFromUserAgent(httpRequest, "");

            var expected = build.HasValue ? new Version(major, minor, build.Value) : new Version(major, minor);
            Assert.That(version, Is.EqualTo(expected));
        }

        [TestCase("Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:47.0) Gecko/20100101 Firefox/47.0")]
        [TestCase("org.worldbank.solutions.interviewer/abc (build 1)")]
        [TestCase("")]
        public void when_get_user_agent_without_app_version_should_return_null(string userAgent)
        {
            IHeaderDictionary headers = new HeaderDictionary();
            headers.Append("User-Agent", userAgent);
            var httpRequest = Mock.Of<HttpRequest>(r => r.Headers == headers);

            Assert.That(WB.UI.Headquarters.Code.Extensions.GetProductVersionFromUserAgent(httpRequest, "org.worldbank"), Is.Null);
        }
    }
}
