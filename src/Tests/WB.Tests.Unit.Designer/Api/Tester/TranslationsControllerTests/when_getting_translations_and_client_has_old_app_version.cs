using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.Api.Tester.TranslationsControllerTests
{
    public class when_getting_translations_and_client_has_old_app_version : TranslationsControllerTestsContext
    {
        [Test]
        public async Task should_response_code_be_UpgradeRequired()
        {
            var controller = CreateTranslationsController();
            var statusCodeResult = 
                await controller.Get(new QuestionnaireRevision(Id.g1), version: ApiVersion.CurrentTesterProtocolVersion - 1);

            Assert.That(statusCodeResult,
                Has.Property(nameof(StatusCodeResult.StatusCode))
                   .EqualTo((int)HttpStatusCode.UpgradeRequired));
        }
    }
}
