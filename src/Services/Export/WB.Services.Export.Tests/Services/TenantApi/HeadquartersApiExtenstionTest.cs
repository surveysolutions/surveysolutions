using System;
using System.Net.Http;
using NUnit.Framework;
using Refit;
using WB.Services.Export.Services;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.Tests.Services.TenantApi
{
    public class HeadquartersApiExtenstionTest
    {
        [Test]
        public void should_return_base_url_configured_with_refit()
        {
            string baseAddress = $"http://test.com/{Guid.NewGuid()}";
            var service = RestService.For<IHeadquartersApi>(new HttpClient
            {
                BaseAddress = new Uri(baseAddress)
            });

            Assert.That(service.BaseUrl(), Is.EqualTo(baseAddress));
        }
    }
}