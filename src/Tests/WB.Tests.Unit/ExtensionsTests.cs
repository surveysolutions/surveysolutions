using System;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;

namespace WB.Tests.Unit
{
    [TestFixture]
    public class ExtensionsTests
    {
        [TestCase(47, 0, null, "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:47.0) Gecko/20100101 Firefox/47.0")]
        [TestCase(42, 0, null, "Mozilla/5.0 (Macintosh; Intel Mac OS X x.y; rv:42.0) Gecko/20100101 Firefox/42.0")]
        [TestCase(51, 0, 2704, "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36")]
        [TestCase(38, 0, 2220, "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.106 Safari/537.36 OPR/38.0.2220.41")]
        [TestCase(10, 0, null, "Mozilla/5.0 (iPhone; CPU iPhone OS 10_3_1 like Mac OS X) AppleWebKit/603.1.30 (KHTML, like Gecko) Version/10.0 Mobile/14E304 Safari/602.1")]
        [TestCase(9, 0, null, "Mozilla/5.0 (compatible; MSIE 9.0; Windows Phone OS 7.5; Trident/5.0; IEMobile/9.0)")]
        [TestCase(2, 1, null, "Googlebot/2.1 (+http://www.google.com/bot.html)")]
        public void when_get_user_agent_with_version(int major, int minor, int? build, string userAgent)
        {
            IHeaderDictionary headers = new HeaderDictionary();
            headers.Add("User-Agent", userAgent);
            var httpRequest = Mock.Of<HttpRequest>(r => r.Headers == headers);
            var version = WB.UI.Headquarters.Code.Extensions.GetProductVersionFromUserAgent(httpRequest, "");

            var expected = build.HasValue ? new Version(major, minor, build.Value) : new Version(major, minor);
            Assert.That(version, Is.EqualTo(expected));
        }
    }
}
