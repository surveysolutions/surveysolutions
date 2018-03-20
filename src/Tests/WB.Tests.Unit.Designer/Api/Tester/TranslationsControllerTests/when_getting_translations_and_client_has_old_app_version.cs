using System;
using System.Net;
using System.Web.Http;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;

namespace WB.Tests.Unit.Designer.Api.Tester.TranslationsControllerTests
{
    public class when_getting_translations_and_client_has_old_app_version : TranslationsControllerTestsContext
    {
        [Test]
        public void should_response_code_be_UpgradeRequired()
        {
            var controller = CreateTranslationsController();
            var expectedException =
                Assert.Throws<HttpResponseException>(() => controller.Get(Guid.Parse("11111111111111111111111111111111"), version: ApiVersion.CurrentTesterProtocolVersion - 1));

            expectedException.Response.StatusCode.Should().Be(HttpStatusCode.UpgradeRequired);
        }
    }
}
