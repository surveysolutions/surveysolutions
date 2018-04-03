using System.Net;
using System.Web.Http;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;

namespace WB.Tests.Unit.Designer.Api.Tester.QuestionnairesControllerTests
{
    public class when_getting_list_of_questionaires_and_client_has_old_app_version : QuestionnairesControllerTestsContext
    {
        [Test]
        public void should_response_code_be_UpgradeRequired()
        {
            var controller = CreateQuestionnairesController();
            var expectedException =
                Assert.Throws<HttpResponseException>(() => controller.Get(version: ApiVersion.CurrentTesterProtocolVersion - 1, pageIndex: 1, pageSize: 128));

            expectedException.Response.StatusCode.Should().Be(HttpStatusCode.UpgradeRequired);
        }
    }
}
